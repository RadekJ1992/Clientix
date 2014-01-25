﻿using AddressLibrary;
using Packet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Clientix {

    public partial class Clientix : Form {

        delegate void SetTextCallback(string text);
        delegate void ConnectionEstablishedCallback(String clientName, int port, int vpi, int vci);
        delegate void ConnectionBrokenCallback(int port, int vpi, int vci);

        //otrzymany i wysyłany pakiets
        private Packet.ATMPacket receivedPacket;
        //private Packet.ATMPacket processedPacket;
        public class Route {
            public Address destAddr;
            public int bandwidth;
            public int port;

            public Route(Address addr, int band, int port) {
                destAddr = addr;
                bandwidth = band;
                this.port = port;
            }
        }
        public List<Route> routeList;
        //kolejka pakietów stworzona z wysyłanej wiadomości
        private Queue<Packet.ATMPacket> packetsFromString;

        //kolejka pakietów odebranych z chmury
        private Queue<Packet.ATMPacket> queuedReceivedPackets = new Queue<Packet.ATMPacket>();

        //dane chmury
        private IPAddress cloudAddress;        //Adres na którym chmura nasłuchuje
        private Int32 cloudPort;           //port chmury

        //dane zarządcy
        private IPAddress managerAddress;        //Adres na którym chmura nasłuchuje
        private Int32 managerPort;           //port chmury

        private IPEndPoint cloudEndPoint;
        private IPEndPoint managerEndPoint;

        private bool isClientNumberSet;

        private Address myAddress;

        private Socket cloudSocket;
        public Socket managerSocket { get; private set; }

        private int tempMid;
        //nazwa klienta
        public String username { get; set; }

        private Thread receiveThread;     //wątek służący do odbierania połączeń
        //private Thread sendThread;        // analogicznie - do wysyłania

        //dane chmury
        private IPAddress controlCloudAddress;        //Adres na którym chmura nasłuchuje
        private Int32 controlCloudPort;           //port chmury
        private IPEndPoint controlCloudEndPoint;
        private Socket controlCloudSocket;

        private Thread controlReceiveThread;     //wątek służący do odbierania połączeń
        private Thread controlSendThread;        // analogicznie - do wysyłania

        private Queue _whatToSendQueue;
        private Queue whatToSendQueue;

        private string userToBeCalled;

        // do odbierania
        private NetworkStream networkStream;
        //do wysyłania
        private NetworkStream netStream;

        private NetworkStream controlNetworkStream; //dla sterowania

        public bool isDisconnect;
        public bool isRunning { get; private set; }     //info czy klient chodzi - dla zarządcy

        public bool isConnectedToControlCloud { get; private set; }

        private bool isClientNameSet;
        public bool isConnectedToCloud { get; private set; } // czy połączony z chmurą?
        public bool isConnectedToManager { get; set; } // czy połączony z zarządcą?
        public bool isLoggedToManager { get; set; } // czy zalogowany w zarządcy?
        //tablica innych węzłów klienckich podłączonych do sieci otrzymana do zarządcy
        public List<String> otherClients { get; set; }

        private bool isFirstMouseEnter;
        //słownik klientów, z którymi mamy połączenie i odpowiadających im komvinacji port,vpi,vci
        public Dictionary<String, PortVPIVCI> VCArray { get; set; }

        private Agentix agent; //agent zarządzania

        public Clientix() {
            isDisconnect = false;
            tempMid = 0;
            isClientNumberSet = false;
            InitializeComponent();
            //tooltip dla nazwy klienta
            System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();
            toolTip.SetToolTip(this.label7, "Nazwa klienta może zawierać litery, cyfry i znak '_'");
            toolTip.SetToolTip(this.usernameField, "Nazwa klienta może zawierać litery, cyfry i znak '_'");
            toolTip.AutoPopDelay = 2000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 500;
            toolTip.ShowAlways = true;
            isConnectedToControlCloud = false;
            otherClients = new List<string>();
            VCArray = new Dictionary<String, PortVPIVCI>();
            isFirstMouseEnter = true;
            isClientNameSet = false;
            isLoggedToManager = false;
            routeList = new List<Route>();
            _whatToSendQueue = new Queue();
            whatToSendQueue = Queue.Synchronized(_whatToSendQueue);
            List<int> speedList = new List<int>();
            speedList = new List<int>();
            speedList.Add(2);
            speedList.Add(6);
            speedList.Add(10);
            BindingSource bs = new BindingSource();
            bs.DataSource = speedList;
            clientSpeedBox.DataSource = bs;
            selectedClientBox.DataSource = otherClients;
        }

        private void sendMessage(object sender, EventArgs e) {
            packetsFromString = Packet.AAL.getATMPackets(enteredTextField.Text);
            if (!isConnectedToCloud) log.AppendText("Nie jestem połączony z chmurą!!");
            else {
                foreach (Packet.ATMPacket packet in packetsFromString) {
                    netStream = new NetworkStream(cloudSocket);
                    PortVPIVCI temp;
                    if (VCArray.TryGetValue((String)selectedClientBox.SelectedItem, out temp)) {
                        SetText("Wysyłam pakiet do " + (String)selectedClientBox.SelectedItem + " z ustawieniem [" + temp.port + ";" +
                                            temp.VPI + ";" + temp.VCI + "] o treści: " + Packet.AAL.GetStringFromBytes(packet.payload)+"\n");
                        packet.port = temp.port;
                        packet.VPI = temp.VPI;
                        packet.VCI = temp.VCI;
                        BinaryFormatter bformatter = new BinaryFormatter();
                        bformatter.Serialize(netStream, packet);
                        netStream.Close();
                    }
                }
            }
            enteredTextField.Clear();
        }

        private void sendMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            //jeśli naciśniesz enter; żeby to działało to wystarczy jeszcze tylko podlinkować zdażenie pod pole tekstowe (zrobione)
            if (sendText.Enabled && e.KeyChar.Equals((char)Keys.Enter)) sendMessage(sender, e);
        }

        private void connectToCloud(object sender, EventArgs e) {
            if (isClientNumberSet) {
                if (isClientNameSet) {
                    if (!isConnectedToCloud) {
                        if (IPAddress.TryParse(cloudIPField.Text, out cloudAddress)) {
                            log.AppendText("IP ustawiono jako " + cloudAddress.ToString() + " \n");
                        } else {
                            log.AppendText("Błąd podczas ustawiania IP chmury (zły format?)" + " \n");
                        }
                        if (Int32.TryParse(cloudPortField.Text, out cloudPort)) {
                            log.AppendText("Port chmury ustawiony jako " + cloudPort.ToString() + " \n");
                        } else {
                            log.AppendText("Błąd podczas ustawiania portu chmury (zły format?)" + " \n");
                        }

                        cloudSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                        cloudEndPoint = new IPEndPoint(cloudAddress, cloudPort);
                        try {
                            cloudSocket.Connect(cloudEndPoint);
                            isConnectedToCloud = true;
                            receiveThread = new Thread(this.receiver);
                            receiveThread.IsBackground = true;
                            receiveThread.Start();
                        } catch (SocketException) {
                            isConnectedToCloud = false;
                            log.AppendText("Błąd podczas łączenia się z chmurą\n");
                            log.AppendText("Złe IP lub port? Chmura nie działa?\n");
                        }
                    } else SetText("Klient jest już połączony z chmurą\n");
                } else SetText("Musisz najpierw ustalić nazwę klienta!\n");
            } else SetText("Ustaw adres klienta!\n");
        }

        private void connectToManager(object sender, EventArgs e) {
            if (isClientNameSet) {
                if (!isConnectedToManager) {
                    if (IPAddress.TryParse(managerIPField.Text, out managerAddress)) {
                        log.AppendText("IP zarządcy ustawione jako " + managerAddress.ToString() + " \n");
                    } else {
                        log.AppendText("Błąd podczas ustawiania IP zarządcy\n");
                    }
                    if (Int32.TryParse(managerPortField.Text, out managerPort)) {
                        log.AppendText("Port zarządcy ustawiony jako " + managerPort.ToString() + " \n");
                    } else {
                        log.AppendText("Błąd podczas ustawiania portu zarządcy\n");
                    }

                    managerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    managerEndPoint = new IPEndPoint(managerAddress, managerPort);
                    try {
                        managerSocket.Connect(managerEndPoint);
                        isConnectedToManager = true;
                        agent = new Agentix(this);
                        agent.readThread.Start();
                        agent.readThread.IsBackground = true;
                        agent.writeThread.Start();
                        agent.writeThread.IsBackground = true;
                        agent.sendLoginC = true;
                    } catch (SocketException) {
                        isConnectedToManager = false;
                        log.AppendText("Błąd podczas łączenia się z zarządcą!\n");
                        log.AppendText("Złe IP lub port? Zarządca nie działa?\n");
                    }
                } else SetText("Już jestem połączony z zarządcą!\n");
            } else SetText("Ustal nazwę klienta!\n");
        }


        private void receiver() {
            try {
                if (networkStream == null) {
                    networkStream = new NetworkStream(cloudSocket);
                    //tworzy string 'client ' i tu jego nazwę
                    String welcomeString = "Client " + username + " " + myAddress.ToString();
                    //tworzy tablicę bajtów z tego stringa
                    byte[] welcomeStringBytes = AAL.GetBytesFromString(welcomeString);
                    //wysyła tą tablicę bajtów streamem
                    networkStream.Write(welcomeStringBytes, 0, welcomeStringBytes.Length);
                }
                BinaryFormatter bf = new BinaryFormatter();
                receivedPacket = (Packet.ATMPacket)bf.Deserialize(networkStream);
                int tempSeq = 0;

                PortVPIVCI temp = new PortVPIVCI(receivedPacket.port, receivedPacket.VPI, receivedPacket.VCI);
                String tempName = "";
                bool isNameFound = false;
                foreach (String name in VCArray.Keys) {
                    PortVPIVCI t;
                    VCArray.TryGetValue(name, out t);
                    if (t.port == temp.port && t.VPI == temp.VPI && t.VCI == temp.VCI) {
                        tempName = name;
                        isNameFound = true;
                    }
                }
                /*
                if (isNameFound) {
                    SetText(tempName+ " :  ");
                } else SetText("[" + receivedPacket.port + ";" + receivedPacket.VPI + ";" + receivedPacket.VCI + "] : ");
                */
                // gdy wiadomość zawarta jest w jednym pakiecie
                if (receivedPacket.PacketType == Packet.ATMPacket.AALType.SSM) {
                    if (isNameFound) {
                        SetText(tempName + " :  ");
                    } else SetText("[" + receivedPacket.port + ";" + receivedPacket.VPI + ";" + receivedPacket.VCI + "] : ");
                    SetText(Packet.AAL.getStringFromPacket(receivedPacket) + "\n");
                    tempMid = 0;
                }
                else if (receivedPacket.PacketType == Packet.ATMPacket.AALType.BOM) {
                    if (isNameFound) {
                        SetText(tempName + " :  ");
                    } else SetText("[" + receivedPacket.port + ";" + receivedPacket.VPI + ";" + receivedPacket.VCI + "] : ");
                    tempSeq = 0;
                    tempMid = receivedPacket.AALMid;
                    SetText(Packet.AAL.getStringFromPacket(receivedPacket));
                    /*
                    queuedReceivedPackets.Clear();
                    queuedReceivedPackets.Enqueue(receivedPacket);
                    */
                }
                else if (receivedPacket.PacketType == Packet.ATMPacket.AALType.COM) {
                    if (receivedPacket.AALMid == tempMid) {
                        //sprawdza kolejnosc AALSeq

                        //usun tempmid
                        if (receivedPacket.AALSeq == ++tempSeq) {
                            SetText(Packet.AAL.getStringFromPacket(receivedPacket));
                            //queuedReceivedPackets.Enqueue(receivedPacket);
                        }
                        else {
                            //SetText("\nPakiet ma inny AALSeq niż powinien mieć, pakiety przyszły w innej kolejności!\n");
                            SetText(Packet.AAL.getStringFromPacket(receivedPacket));
                        }
                    }
                    else {
                        //SetText("\nPakiet z innej wiadomości! Inne AALMid!\n");
                        SetText("\n" + tempName + " : " + Packet.AAL.getStringFromPacket(receivedPacket));
                    }
                }
                else if (receivedPacket.PacketType == Packet.ATMPacket.AALType.EOM) {
                    /*
                    queuedReceivedPackets.Enqueue(receivedPacket);
                    SetText(Packet.AAL.getStringFromPackets(queuedReceivedPackets));
                    queuedReceivedPackets.Clear();
                     */
                    SetText(Packet.AAL.getStringFromPacket(receivedPacket) + "\n");
                    tempSeq = 0;
                    tempMid = 0;
                }
                //networkStream.Close();
                receiver();
            } catch (Exception e){
                if (isDisconnect) { 
                    SetText("Rozłączam się z chmurą!\n"); isDisconnect = false; networkStream = null; 
                } else {
                    SetText("Coś poszło nie tak : " + e.Message + "\n");
                    cloudSocket = null;
                    cloudEndPoint = null;
                    networkStream = null;
                    isConnectedToCloud = false;
                }
            }
        }

        public void SetText(string text) {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (this.log.InvokeRequired) {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else {
                try {
                    this.log.AppendText(text);
                } catch { }
            }
        }

        private void setUsernameButton_Click(object sender, EventArgs e) {
            if (!usernameField.Text.Equals("")) {
                if (isConnectedToControlCloud) {
                    if (Regex.IsMatch(usernameField.Text, "^[a-zA-Z0-9_]+$")) {
                        username = usernameField.Text;
                        List<String> _msgList = new List<String>();
                        _msgList.Add("LOGIN");
                        _msgList.Add(username);
                        _msgList.Add(myAddress.ToString());
                        SPacket welcomePacket = new SPacket(myAddress.ToString(), new Address(0, 0, 0).ToString(), _msgList);
                        whatToSendQueue.Enqueue(welcomePacket);
                        SetText("Nazwa klienta ustawiona jako " + username + "\n");
                    } else SetText("Połącz z chmurą zarządania!\n");
                } else this.SetText("Dawaj jakąś ludzką nazwę (dozwolone tylko litery, cyfry i znak '_')\n");
            } else {
                SetText("Dawaj jakąś ludzką nazwę (dozwolone tylko litery, cyfry i znak '_')\n");
                isClientNameSet = false;
            }
        }

        private void getOtherClients_Click(object sender, EventArgs e) {
            if (agent != null) agent.sendGetClients = true;
            if (isConnectedToControlCloud) {

            }
        }

        public void setOtherClients(List<String> otherCl) {
            List<String> temp = new List<String>();
            //usun swoje wlasne imie
            foreach (String name in otherCl) {
                if (name != username) {
                    temp.Add(name);
                }
            }
            otherClients = temp;
            BindingSource bs = new BindingSource();
            bs.DataSource = otherClients;
            this.Invoke((MethodInvoker)delegate() {
            selectedClientBox.DataSource = bs;
            });
        }
        private void connectWithClientButton_Click(object sender, EventArgs e) {
            if (agent != null) {
                if ((String)selectedClientBox.SelectedItem != null) {
                    String clientName = (String)selectedClientBox.SelectedItem;
                    agent.whoIsCalled = clientName;
                    agent.sendCall = true;
                    SetText("Wysłano żądanie nawiązania połączenia z " + clientName + "\n");
                } else {
                    SetText("Nie wybrano klienta\n");
                }
            }
            if (isConnectedToControlCloud) {
                if ((String)selectedClientBox.SelectedItem != null) {
                    String clientName = (String)selectedClientBox.SelectedItem;
                    userToBeCalled = clientName;
                    List<String> _msgList = new List<String>();
                    _msgList.Add("REQ_CONN");
                    _msgList.Add(userToBeCalled);
                    _msgList.Add((string)clientSpeedBox.SelectedItem);
                    SPacket welcomePacket = new SPacket(myAddress.ToString(), new Address(0, 0, 0).ToString(), _msgList);
                    whatToSendQueue.Enqueue(welcomePacket);
                } else {
                    SetText("Nie wybrano klienta\n");
                }
            }
        }

        //metoda wywołana gdy agent odbierze wiadomość ESTABLISHED clientNAME port vpi vci
        public void connectionEstablished(String clientName, int port, int vpi, int vci) {
            if (this.InvokeRequired) {
                ConnectionEstablishedCallback d = new ConnectionEstablishedCallback(connectionEstablished);
                this.Invoke(d, new object[] { clientName, port, vpi, vci });
            } else {
                if (otherClients.Count == 0) {
                    otherClients.Add(clientName);
                    
                        VCArray.Add(clientName, new PortVPIVCI(port, vpi, vci));
                    
                }
                try {
                    foreach (String name in otherClients) {
                        if (name == clientName) {
                            if (!VCArray.ContainsKey(clientName)) {

                                VCArray.Add(clientName, new PortVPIVCI(port, vpi, vci));

                            } else {
                                try {
                                    VCArray.Remove(clientName);
                                } catch { }
                                VCArray.Add(clientName, new PortVPIVCI(port, vpi, vci));

                            }
                        } else {
                            otherClients.Add(clientName);
                            VCArray.Add(clientName, new PortVPIVCI(port, vpi, vci));
                        }
                        //sprawdza przy okazji czy połączenie zostało nawiązane z aktualnie zaznaczonym klientem - jeśli tak - aktywuje możliwość wysyłania wiadomości
                        String tempSelCl = "";
                        if (selectedClientBox.SelectedItem != null) tempSelCl = (String)selectedClientBox.SelectedItem;
                        if (VCArray.ContainsKey(tempSelCl)) {
                            disconnectWithClient.Enabled = true;
                            sendText.Enabled = true;
                        } else {
                            disconnectWithClient.Enabled = false;
                            sendText.Enabled = false;
                        }
                        SetText("Połączenie z " + clientName + " zostało nawiązane!\n");
                        this.Refresh();
                    }
                } catch (InvalidOperationException) { } catch (ArgumentException) { }
            }
        }

        public void connectionBroken(int port, int vpi, int vci) {
            if (this.InvokeRequired) {
                ConnectionBrokenCallback d = new ConnectionBrokenCallback(connectionBroken);
                this.Invoke(d, new object[] { port, vpi, vci });
            } else {
                PortVPIVCI temp = new PortVPIVCI(port, vpi, vci);
                String tempName = "";
                foreach (String name in VCArray.Keys) {
                    PortVPIVCI t;
                    VCArray.TryGetValue(name, out t);
                    if (t.port == temp.port && t.VPI == temp.VPI && t.VCI == temp.VCI) {
                        tempName = name;
                    }
                }
                        VCArray.Remove(tempName);
                        disconnectWithClient.Enabled = false;
                        sendText.Enabled = false;
                        SetText("Połączenie z " + tempName + " zostało zerwane! \n");
            }
        }

        private void disconnectWithClient_Click(object sender, EventArgs e) {
            if (agent != null) {
                agent.whoToDisconnect = ((String)selectedClientBox.SelectedItem);
                agent.sendDisconnect = true;
                SetText("Wysyłam żądanie zerwania połączenia z " + ((String)selectedClientBox.SelectedItem) + "\n");
            }
        }

        private void selectedClientBoxs_SelectedIndexChanged(object sender, EventArgs e) {
            //jeśli jest połączenie z tym klientem - pojawia się opcja usunięcia połączenia
            if (VCArray.ContainsKey((String)selectedClientBox.SelectedItem)) {
                disconnectWithClient.Enabled = true;
                sendText.Enabled = true;
            } else {
                disconnectWithClient.Enabled = false;
                sendText.Enabled = false;
            }
        }
        public void readConfig(String clientName) {
            try {
                username = clientName;
                usernameField.Text = clientName;
                isClientNameSet = true;
                SetText("Ustalam nazwę klienta jako " + username + "\n");
                String path = "config" + username + ".txt";
                otherClients = new List<String>();
                using (StreamReader sr = new StreamReader(path)) {
                    string[] lines = System.IO.File.ReadAllLines(path);
                    foreach (String line in lines) {
                        String[] command = line.Split(' ');
                        if (command[0] == "ADD_CONNECTION") {
                            try {
                                VCArray.Add(command[1], new PortVPIVCI(int.Parse(command[2]), int.Parse(command[3]), int.Parse(command[4])));
                                if (!otherClients.Contains(command[1])) {
                                    otherClients.Add(command[1]);
                                    SetText("Dodaję klienta " + command[1] + "\n");
                                }
                                SetText("Dodaję połączenie z klientem " + command[1] + " na porcie " 
                                    + command[2] + " VPI " + command[3] + " VCI " + command[4] + "\n");
                            } catch (IndexOutOfRangeException) {
                                SetText("Komenda została niepoprawnie sformułowana (za mało parametrów)\n");
                            }
                        } else if (command[0] == "ADD_CLIENT") {
                            try {
                                otherClients.Add(command[1]);
                                SetText("Dodaję klienta " + command[1] + "\n");
                            } catch (IndexOutOfRangeException) {
                                SetText("Komenda została niepoprawnie sformułowana (za mało parametrów)\n");
                            }
                        } else if (command[0] == "ADD_ROUTE") {
                            Address adr;
                            int port;
                            int band;
                            if (int.TryParse(command[1], out port)) {
                                if (Address.TryParse(command[2], out adr)) {
                                    if (int.TryParse(command[3], out band)) {
                                        routeList.Add(new Route(adr,band,port));
                                    } else SetText("Zły format danych\n");
                                }else SetText("Zły format danych\n");
                            }else SetText("Zły format danych\n");
                        }
                    }
                }
            } catch (Exception exc) {
                SetText("Błąd podczas konfigurowania pliku konfiguracyjnego\n");
                SetText(exc.Message + "\n");
            }
        }

        private void selectedClientBox_MouseEnter(object sender, EventArgs e) {
            if (isFirstMouseEnter) {
            setOtherClients(otherClients);
            isFirstMouseEnter = false;
            }
        }

        private void SaveConfigButton_Click(object sender, EventArgs e) {
            saveConfig();
        }

        private void saveConfig() {
            if (username != null) {
                List<String> lines = new List<String>();
                foreach (String client in VCArray.Keys) {
                    PortVPIVCI value;
                    if (VCArray.TryGetValue(client, out value)) lines.Add("ADD_CONNECTION " + client +
                                                                        " " + value.port + " " + value.VPI + " " + value.VCI);
                }
                foreach (String client in otherClients) {
                    if (!VCArray.ContainsKey(client)) lines.Add("ADD_CLIENT " + client);
                }
                foreach (Route rt in routeList) {
                    lines.Add("ADD_ROUTE " + rt.port + " " + rt.destAddr.ToString() + " " + rt.bandwidth);
                }
                System.IO.File.WriteAllLines("config" + username + ".txt", lines);
                SetText("Zapisuję ustawienia do pliku config" + username + ".txt\n");
            } else SetText("Ustal nazwę klienta!\n");
        }

        private void DisconnectButton_Click(object sender, EventArgs e) {
            isDisconnect = true;
            isConnectedToCloud = false;
            isConnectedToManager = false;
            if (cloudSocket != null) cloudSocket.Close();
            if (managerSocket != null) managerSocket.Close();
        }

        private void Clientix_FormClosed(object sender, FormClosedEventArgs e) {
            if (username != null) saveConfig();
        }

        private void setClientNumber_Click(object sender, EventArgs e) {
            try {
                int clientAddressNetwork = int.Parse(ClientNetworkNumberField.Text);
                int clientAddressSubnet = int.Parse(ClientSubnetworkNumberField.Text);
                int clientAddressHost = int.Parse(ClientHostNumberField.Text);
                isClientNumberSet = true;
                myAddress = new Address(clientAddressNetwork, clientAddressSubnet, clientAddressHost);
                SetText("Adres klienta ustawiony jako " + myAddress.ToString() + "\n");
            } catch {
                isClientNumberSet = false;
                SetText("Błędne dane wejściowe\n");
            }
        }

        private void conToCloudButton_Click(object sender, EventArgs e) {
            if (!isConnectedToControlCloud) {
                if (isClientNumberSet) {
                    if (IPAddress.TryParse(controlCloudIPTextBox.Text, out controlCloudAddress)) {
                        SetText("IP ustawiono jako " + controlCloudAddress.ToString() + "\n");
                    } else {
                        SetText("Błąd podczas ustawiania IP chmury (zły format?)\n");
                    }
                    if (Int32.TryParse(controlCloudPortTextBox.Text, out controlCloudPort)) {
                        SetText("Port chmury ustawiony jako " + controlCloudPort.ToString() + "\n");
                    } else {
                        SetText("Błąd podczas ustawiania portu chmury (zły format?)\n");
                    }

                    controlCloudSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    controlCloudEndPoint = new IPEndPoint(cloudAddress, cloudPort);
                    try {
                        controlCloudSocket.Connect(controlCloudEndPoint);
                        isConnectedToControlCloud = true;
                        controlNetworkStream = new NetworkStream(controlCloudSocket);
                        List<String> _welcArr = new List<String>();
                        _welcArr.Add("HELLO");
                        SPacket welcomePacket = new SPacket(myAddress.ToString(), new Address(0, 0, 0).ToString(), _welcArr);
                        whatToSendQueue.Enqueue(welcomePacket);
                        //whatToSendQueue.Enqueue("HELLO " + myAddr);
                        controlReceiveThread = new Thread(this.controlReceiver);
                        controlReceiveThread.IsBackground = true;
                        controlReceiveThread.Start();
                        controlSendThread = new Thread(this.controlSender);
                        controlSendThread.IsBackground = true;
                        controlSendThread.Start();
                        conToCloudButton.Text = "Rozłącz";
                        SetText("Połączono!\n");
                    } catch (SocketException) {
                        isConnectedToControlCloud = false;
                        SetText("Błąd podczas łączenia się z chmurą\n");
                        SetText("Złe IP lub port? Chmura nie działa?\n");
                    }
                } else {
                    SetText("Wprowadź numery sieci i podsieci\n");
                }
            } else {
                isConnectedToCloud = false;
                conToCloudButton.Text = "Połącz";
                SetText("Rozłączono!\n");
                if (cloudSocket != null) cloudSocket.Close();
            }
        }
        /// <summary>
        /// wątek odbierający wiadomości z chmury
        /// </summary>
        public void controlReceiver() {
            while (isConnectedToControlCloud) {
                BinaryFormatter bf = new BinaryFormatter();
                try {
                    SPacket receivedPacket = (Packet.SPacket)bf.Deserialize(controlNetworkStream);
                    //_msg = reader.ReadLine();
                    SetText("Odczytano:\n" + receivedPacket.ToString() + "\n");

                    if (receivedPacket.getParames()[0] == "OK" && receivedPacket.getSrc() == "0.0.0") {
                        isClientNameSet = true;
                    } else if (receivedPacket.getParames()[0] == "NAME_TAKEN" && receivedPacket.getSrc() == "0.0.0") {
                        SetText("Nazwa użytkownika zajęta, wybierz inną!;");
                        username = null;
                    } else if (receivedPacket.getParames()[0] == "CLIENTS" && receivedPacket.getSrc() == "0.0.0") {
                        List<string> _temp = receivedPacket.getParames();
                        _temp.Remove("CLIENTS");
                        setOtherClients(_temp);
                    } else if (receivedPacket.getParames()[0] == "YES" && receivedPacket.getSrc() == "0.0.0") {
                        Address calledAddress = Address.Parse(receivedPacket.getParames()[1]);
                        string temp = "REQ_CALL " + calledAddress.ToString();
                        SPacket pck = new SPacket(myAddress.ToString(), "0.0.1", temp);
                        whatToSendQueue.Enqueue(pck);
                    } else if (receivedPacket.getParames()[0] == "NO" && receivedPacket.getSrc() == "0.0.0") {
                        SetText("Nie masz uprawnień do wykonania takiego połączenia!\n");
                        userToBeCalled = null;
                    } else {
                        /*
                         * 
                         * 
                         * 
                         * 
                         *  tutaj przekazać pakiet do LRMA
                         * 
                         * 
                         * 
                         * 
                         */
                    }
                } catch {
                    SetText("WUT");
                }
            }
        }
        /// <summary>
        /// wątek wysyłający wiadomości do chmury
        /// </summary>
        public void controlSender() {
            while (isConnectedToCloud) {
                //jeśli coś jest w kolejce - zdejmij i wyślij
                if (whatToSendQueue.Count != 0) {
                    SPacket _pck = (SPacket)whatToSendQueue.Dequeue();
                    BinaryFormatter bformatter = new BinaryFormatter();
                    bformatter.Serialize(networkStream, _pck);
                    networkStream.Flush();
                    String[] _argsToShow = _pck.getParames().ToArray();
                    String argsToShow = "";
                    foreach (String str in _argsToShow) {
                        argsToShow += str + " ";
                    }
                    SetText("Wysłano: " + _pck.getSrc() + ":" + _pck.getDest() + ":" + argsToShow + "\n");
                }
            }
        }
    }
    class Agentix {
        StreamReader read = null;
        StreamWriter write = null;
        NetworkStream netstream = null;
        Clientix parent;
        public Thread writeThread;
        public Thread readThread;
        public bool sendLoginC;
        public bool sendCall;
        public String whoIsCalled;
        public bool sendDisconnect;
        public String whoToDisconnect;
        public bool sendGetClients;

        public Agentix(Clientix parent) {
            this.parent = parent;
            netstream = new NetworkStream(parent.managerSocket);
            read = new StreamReader(netstream);
            write = new StreamWriter(netstream);
            sendLoginC = false;
            sendCall = false;
            sendDisconnect = false;
            sendGetClients = false;
            whoIsCalled = "";
            whoToDisconnect = "";
            writeThread = new Thread(writer);
            readThread = new Thread(reader);
        }
        //Funkcja przesyłająca dane do serwera
        //Wykonywana w osobnym watku
        private void writer() {
            while (parent.isConnectedToManager) {
                try {
                    if (sendLoginC) {
                        write.WriteLine("LOGINC\n" + parent.username);
                        write.Flush();
                        sendLoginC = false;
                    }
                    if (sendCall) {
                        if (whoIsCalled != "" && whoIsCalled != null) {
                            write.WriteLine("CALL\n" + whoIsCalled);
                            write.Flush();
                            whoIsCalled = "";
                            sendCall = false;
                        }
                    }
                    if (sendDisconnect) {
                        if (whoToDisconnect != "" && whoToDisconnect != null) {
                            write.WriteLine("DISCONNECT\n" + whoToDisconnect);
                            write.Flush();
                            whoToDisconnect = "";
                            sendDisconnect = false;
                        }
                    }
                    if (sendGetClients) {
                        write.WriteLine("GET_CLIENTS\n");
                        write.Flush();
                        sendGetClients = false;
                    }
                } catch {
                    parent.isConnectedToManager = false;
                    writeThread.Abort();
                    readThread.Abort();
                }
            }
        }
        //Funkcja odpowiedzialna za odbieraie danych od serwera
        //wykonywana w osobnym watąku
        private void reader() {

            String odp;
            Char[] delimitter = { ' ' };
            String[] slowa;
            while (parent.isConnectedToManager) {
                try {
                    odp = read.ReadLine();
                    Console.WriteLine("Odczytano: " + odp);
                    slowa = odp.Split(delimitter, StringSplitOptions.RemoveEmptyEntries);
                    if (slowa[0] == "LOGGED") {
                        parent.isLoggedToManager = true;
                        parent.SetText("Zalogowano u zarządcy\n");
                    } else if (slowa[0] == "ESTABLISHED") {
                        if (!parent.otherClients.Contains(slowa[1])) this.sendGetClients = true;
                        parent.connectionEstablished(slowa[1], int.Parse(slowa[2]), int.Parse(slowa[3]), int.Parse(slowa[4]));
                    } else if (slowa[0] == "CLIENTS") {
                        List<String> listakl = new List<string>();
                        for (int i = 1; i < slowa.Length; i++) {
                            listakl.Add(slowa[i]);
                        }
                        parent.otherClients = listakl;
                        parent.SetText("Wykryto " + (slowa.Length - 2) + " innych klientów\n");
                        parent.setOtherClients(listakl);
                    } else if (slowa[0] == "MSG" || slowa[0] == "DONE") {
                        parent.SetText("Wykryto komunikat o treści:");
                        foreach (String s in slowa) {
                            parent.SetText(" " + s + " ");
                        }
                        parent.SetText("\n");
                    } else if (slowa[0] == "ERR") {
                        parent.SetText("Wykryto komunikat błędu o treści:");
                        foreach (String s in slowa) {
                            parent.SetText(" " + s + " ");
                        }
                        parent.SetText("\n");
                        parent.isConnectedToManager = false;
                        writeThread.Abort();
                        readThread.Abort();
                        parent.SetText("Połącz się ponownie!\n");
                    } else if (slowa[0] == "DELETE") {
                        try {
                            int tempPort = int.Parse(slowa[1]);
                            int tempVPI = int.Parse(slowa[2]);
                            int tempVCI = int.Parse(slowa[3]);
                            parent.connectionBroken(tempPort, tempVPI, tempVCI);
                        } catch {
                            parent.SetText("Zarządca wysłał złe parametry zerwania połączenia");
                        }
                    }
                } catch (Exception e) {
                    if (parent.isDisconnect) {
                        parent.SetText("Rozłączam się z zarządcą!\n");
                        parent.isConnectedToManager = false;
                        writeThread.Abort();
                        readThread.Abort();
                        parent.isDisconnect = false;
                    } else {
                        parent.SetText(e.Message + "\n");
                        parent.SetText("Problem w połączeniu się z zarządcą :<\n");
                        parent.isConnectedToManager = false;
                        writeThread.Abort();
                        readThread.Abort();
                    }
                }
            }
        }
    }
}
