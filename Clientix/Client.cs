using AddressLibrary;
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
            public List<int> VPIList;

            public Route(Address addr, int band, int port, List<int> VPIList) {
                destAddr = addr;
                bandwidth = band;
                this.port = port;
                this.VPIList = VPIList;
            }
        }
        public List<Route> routeList;
        //kolejka pakietów stworzona z wysyłanej wiadomości
        private Queue<Packet.ATMPacket> packetsFromString;

        //kolejka pakietów odebranych z chmury
        public Queue<Packet.ATMPacket> queuedReceivedPackets = new Queue<Packet.ATMPacket>();

        private Queue _packetsToSend;
        public Queue packetsToSend;
        //dane chmury
        private IPAddress cloudAddress;        //Adres na którym chmura nasłuchuje
        private Int32 cloudPort;           //port chmury

        private Dictionary<String, PortVPIVCI> howToSendDict;
        private Dictionary<string, Address> userDict;
        //dane zarządcy
        private IPAddress managerAddress;        //Adres na którym chmura nasłuchuje
        private Int32 managerPort;           //port chmury

        private IPEndPoint cloudEndPoint;
        private IPEndPoint managerEndPoint;

        private bool isClientNumberSet;

        public Address myAddress;

        public string lastCalledUser;
        public Address lastCalledAddress;
        public Dictionary<Address, int> addrCallIDDict;

        private Dictionary<string, List<int>> NEWCONNIDARRAY;
        private Dictionary<string, List<PortVPIVCI>> NEWVCARRAY;

        private Socket cloudSocket;
        public Socket managerSocket { get; private set; }

        private int tempMid;
        //nazwa klienta
        public String username { get; set; }

        private Thread receiveThread;     //wątek służący do odbierania połączeń
        private Thread sendThread;        // analogicznie - do wysyłania

        //dane chmury
        private IPAddress controlCloudAddress;        //Adres na którym chmura nasłuchuje
        private Int32 controlCloudPort;           //port chmury
        private IPEndPoint controlCloudEndPoint;
        private Socket controlCloudSocket;

        private Thread controlReceiveThread;     //wątek służący do odbierania połączeń
        private Thread controlSendThread;        // analogicznie - do wysyłania

        private Queue _whatToSendQueue;
        public Queue whatToSendQueue;

        private string userToBeCalled;

        public PortVPIVCI lastAddedPortVPIVCI;
        // do odbierania
        private NetworkStream networkStream;
        //do wysyłania
        private NetworkStream netStream;

        private NetworkStream controlNetworkStream; //dla sterowania

        public bool isDisconnect;
        public bool isRunning { get; private set; }     //info czy klient chodzi - dla zarządcy

        public int sentPackets;

        public bool connect = false;
        public bool isConnectedToControlCloud { get; private set; }

        private bool isClientNameSet;
        public bool isConnectedToCloud { get; private set; } // czy połączony z chmurą?
        public bool isConnectedToManager { get; set; } // czy połączony z zarządcą?
        public bool isLoggedToManager { get; set; } // czy zalogowany w zarządcy?
        //tablica innych węzłów klienckich podłączonych do sieci otrzymana do zarządcy
        public List<String> otherClients { get; set; }

        private bool isFirstMouseEnter;
        //słownik klientów, z którymi mamy połączenie i odpowiadających im komvinacji port,vpi,vci
        public Dictionary<String, List<PortVPIVCI>> VCArray { get; set; }

        public bool isNameSet;
        public Dictionary<PortVPIVCI, Address> AddrPortVPIVCIArray { get; set; }
        private int exceptionCount;
        private Agentix agent; //agent zarządzania

        private eLReMix LRM;

        public Clientix() {
            isDisconnect = false;
            lastCalledUser = String.Empty;
            exceptionCount = 0;
            sentPackets = 0;
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
            howToSendDict = new Dictionary<string, PortVPIVCI>();
            VCArray = new Dictionary<String, List<PortVPIVCI>>();
            NEWVCARRAY = new Dictionary<string, List<PortVPIVCI>>();
            NEWCONNIDARRAY = new Dictionary<string, List<int>>();
            isFirstMouseEnter = true;
            isClientNameSet = false;
            isLoggedToManager = false;
            addrCallIDDict = new Dictionary<Address, int>(new AddressComparer());
            _packetsToSend = new Queue();
            packetsToSend = Queue.Synchronized(_packetsToSend);
            routeList = new List<Route>();
            _whatToSendQueue = new Queue();
            whatToSendQueue = Queue.Synchronized(_whatToSendQueue);
            List<int> speedList = new List<int>();
            speedList = new List<int>();
            speedList.Add(2);
            speedList.Add(6);
            speedList.Add(10);
            userDict = new Dictionary<string, Address>();
            BindingSource bs = new BindingSource();
            bs.DataSource = speedList;
            clientSpeedBox.DataSource = bs;
            selectedClientBox.DataSource = otherClients;
            isNameSet = false;
            AddrPortVPIVCIArray = new Dictionary<PortVPIVCI, Address>(new PortVPIVCIComparer());
            (new Thread(new ThreadStart(() => {
                Thread.Sleep(1500);
                if (connect) {
                    //connectToCloud(this, new EventArgs());
                    conToCloudButton_Click(this, new EventArgs());
                    connect = false;
                }
            }))).Start();
        }

        private void sender()
        {
            while (isConnectedToCloud)
            {
                if (packetsToSend.Count != 0) { 
                    ATMPacket packet = (ATMPacket)packetsToSend.Dequeue();
                    if (packet.VPI == -1 && packet.VCI == -1)
                    {
                        netStream = new NetworkStream(cloudSocket);
                        BinaryFormatter bformatter = new BinaryFormatter();
                        bformatter.Serialize(netStream, packet);
                        netStream.Close();
                    }
                    else
                    {
                        /*
                        netStream = new NetworkStream(cloudSocket);
                        List<PortVPIVCI> temp;
                        if (VCArray.TryGetValue((String)selectedClientBox.SelectedItem, out temp))
                        {
                            int i = sentPackets % temp.Count;
                            SetText("Wysyłam pakiet do " + (String)selectedClientBox.SelectedItem + " z ustawieniem [" + temp[i].port + ";" +
                                                temp[i].VPI + ";" + temp[i].VCI + "] o treści: " + Packet.AAL.GetStringFromBytes(packet.payload) + "\n");
                            packet.port = temp[i].port;
                            packet.VPI = temp[i].VPI;
                            packet.VCI = temp[i].VCI;
                            BinaryFormatter bformatter = new BinaryFormatter();
                            bformatter.Serialize(netStream, packet);
                            netStream.Close();
                        }
                         */
                        this.Invoke((MethodInvoker)delegate() {
                        
                            netStream = new NetworkStream(cloudSocket);
                            PortVPIVCI _pvv;
                            if (howToSendDict.TryGetValue((string)howToSendComboBox.SelectedItem, out _pvv))
                            {
                                SetText("Wysyłam pakiet z ustawieniem [" + _pvv.port + ";" +
                                                    _pvv.VPI + ";" + _pvv.VCI + "] o treści: " + Packet.AAL.GetStringFromBytes(packet.payload) + "\n");
                            }
                            packet.port = _pvv.port;
                            packet.VPI = _pvv.VPI;
                            packet.VCI = _pvv.VCI;
                            BinaryFormatter bformatter = new BinaryFormatter();
                            bformatter.Serialize(netStream, packet);
                            netStream.Close();
                        });
                    }
                }
                Thread.Sleep(100);
            }
        }

        private void sendMessage(object sender, EventArgs e) {
            packetsFromString = Packet.AAL.getATMPackets(enteredTextField.Text);
            if (!isConnectedToCloud) log.AppendText("Nie jestem połączony z chmurą!!");
            else {
                foreach (Packet.ATMPacket packet in packetsFromString) {
                    /*netStream = new NetworkStream(cloudSocket);
                    List<PortVPIVCI> temp;
                    if (VCArray.TryGetValue((String)selectedClientBox.SelectedItem, out temp)) {
                        int i = sentPackets % temp.Count;
                        SetText("Wysyłam pakiet do " + (String)selectedClientBox.SelectedItem + " z ustawieniem [" + temp[i].port + ";" +
                                            temp[i].VPI + ";" + temp[i].VCI + "] o treści: " + Packet.AAL.GetStringFromBytes(packet.payload)+"\n");
                        packet.port = temp[i].port;
                        packet.VPI = temp[i].VPI;
                        packet.VCI = temp[i].VCI;
                        BinaryFormatter bformatter = new BinaryFormatter();
                        bformatter.Serialize(netStream, packet);
                        netStream.Close();
                    }*/
                    packetsToSend.Enqueue(packet);
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
                            sendThread = new Thread(this.sender);
                            sendThread.IsBackground = true;
                            sendThread.Start();
                            sendText.Enabled = true;
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
                    List<PortVPIVCI> t = new List<PortVPIVCI>();
                    VCArray.TryGetValue(name, out t);
                    if (t.Contains(temp)) {
                        tempName = name;
                        isNameFound = true;
                    }
                }
                foreach (String name in NEWVCARRAY.Keys) {
                    List<PortVPIVCI> t = new List<PortVPIVCI>();
                    NEWVCARRAY.TryGetValue(name, out t);
                    if (t.Contains(temp)) {
                        tempName = name;
                        isNameFound = true;
                    }
                }
                if (receivedPacket.VCI == -1 && receivedPacket.VPI == -1) {
                    LRM.OdczytajATM(receivedPacket);
                } else {
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
                    } else if (receivedPacket.PacketType == Packet.ATMPacket.AALType.BOM) {
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
                    } else if (receivedPacket.PacketType == Packet.ATMPacket.AALType.COM) {
                        if (receivedPacket.AALMid == tempMid) {
                            //sprawdza kolejnosc AALSeq

                            //usun tempmid
                            if (receivedPacket.AALSeq == ++tempSeq) {
                                SetText(Packet.AAL.getStringFromPacket(receivedPacket));
                                //queuedReceivedPackets.Enqueue(receivedPacket);
                            } else {
                                //SetText("\nPakiet ma inny AALSeq niż powinien mieć, pakiety przyszły w innej kolejności!\n");
                                SetText(Packet.AAL.getStringFromPacket(receivedPacket));
                            }
                        } else {
                            //SetText("\nPakiet z innej wiadomości! Inne AALMid!\n");
                            SetText("\n" + tempName + " : " + Packet.AAL.getStringFromPacket(receivedPacket));
                        }
                    } else if (receivedPacket.PacketType == Packet.ATMPacket.AALType.EOM) {
                        /*
                        queuedReceivedPackets.Enqueue(receivedPacket);
                        SetText(Packet.AAL.getStringFromPackets(queuedReceivedPackets));
                        queuedReceivedPackets.Clear();
                         */
                        SetText(Packet.AAL.getStringFromPacket(receivedPacket) + "\n");
                        tempSeq = 0;
                        tempMid = 0;
                    }
                }
                //networkStream.Close();
                Thread.Sleep(100);
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
                        _msgList.Add(myAddress.ToString());
                        _msgList.Add(username);
                        SPacket welcomePacket = new SPacket(myAddress.ToString(), new Address(1, 0, 2).ToString(), _msgList);
                        whatToSendQueue.Enqueue(welcomePacket);
                        SetText("Nazwa klienta ustawiona jako " + username + "\n");
                    } else SetText("Połącz z chmurą zarządania!\n");
                } else this.SetText("Dawaj jakąś ludzką nazwę (dozwolone tylko litery, cyfry i znak '_')\n");
            } else {
                SetText("Najpierw połącz się z chmurą sterowania!\n");
                isClientNameSet = false;
            }
        }

        private void getOtherClients_Click(object sender, EventArgs e) {
            if (agent != null) agent.sendGetClients = true;
            if (isConnectedToControlCloud) {
                List<String> _msgList = new List<String>();
                _msgList.Add("REQ_CLIENTS");
                SPacket welcomePacket = new SPacket(myAddress.ToString(), new Address(1, 0, 2).ToString(), _msgList);
                whatToSendQueue.Enqueue(welcomePacket);
                SetText("Wysłano żądanie pobrania nazwy klientów\n");
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
            bs.DataSource = otherClients;  //i tak tego chuju nie przeczytasz
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
                    if (!userDict.ContainsKey(clientName)) userDict.Add(clientName, new Address(0, 0, 0));
                    userToBeCalled = clientName;
                    lastCalledUser = clientName;
                    List<String> _msgList = new List<String>();
                    _msgList.Add("REQ_CALL");
                    _msgList.Add(userToBeCalled);
                    _msgList.Add((string)clientSpeedBox.SelectedItem.ToString());
                    SPacket welcomePacket = new SPacket(myAddress.ToString(), new Address(1, 0, 2).ToString(), _msgList);
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
                    List<PortVPIVCI> temp = new List<PortVPIVCI>();
                    if (VCArray.TryGetValue(clientName, out temp)) {
                        temp.Add(new PortVPIVCI(port, vpi, vci));
                        VCArray.Remove(clientName);
                        VCArray.Add(clientName, temp);
                    } else {
                        temp.Add(new PortVPIVCI(port, vpi, vci));
                        VCArray.Add(clientName, temp);
                    }
                }
                try {
                    foreach (String name in otherClients) {
                        if (name == clientName) {
                            if (!VCArray.ContainsKey(clientName)) {
                                List<PortVPIVCI> temp = new List<PortVPIVCI>();
                                temp.Add(new PortVPIVCI(port, vpi, vci));
                                VCArray.Add(clientName, temp);

                            } else {
                                List<PortVPIVCI> temp = new List<PortVPIVCI>();
                                if (VCArray.TryGetValue(clientName, out temp)) {
                                    temp.Add(new PortVPIVCI(port, vpi, vci));
                                    VCArray.Remove(clientName);
                                    VCArray.Add(clientName, temp);
                                }
                            }
                        } else {
                            otherClients.Add(clientName);
                            List<PortVPIVCI> temp = new List<PortVPIVCI>();
                            temp.Add(new PortVPIVCI(port, vpi, vci));
                            VCArray.Add(clientName, temp);
                        }
                        //sprawdza przy okazji czy połączenie zostało nawiązane z aktualnie zaznaczonym klientem - jeśli tak - aktywuje możliwość wysyłania wiadomości
                        String tempSelCl = "";
                        if (selectedClientBox.SelectedItem != null) tempSelCl = (String)selectedClientBox.SelectedItem;
                        if (VCArray.ContainsKey(tempSelCl)) {
                            disconnectWithClient.Enabled = true;
                            sendText.Enabled = true;
                        } else {
                            disconnectWithClient.Enabled = true;
                            sendText.Enabled = false;
                        }
                        List<PortVPIVCI> temp1 = new List<PortVPIVCI>();
                        VCArray.TryGetValue(clientName, out temp1);
                        SetText("Połączenie z " + clientName + " zostało nawiązane na porcie" + port + " VPI " + vpi + " VCI " + vci +". Przepustowość połączenia wynosi " + temp1.Count*2 + " Mbit/s\n");
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
                List<PortVPIVCI> tempList = new List<PortVPIVCI>();
                foreach (String name in VCArray.Keys) {
                    List<PortVPIVCI> t;
                    VCArray.TryGetValue(name, out t);
                    if (t.Contains(temp)) {
                        tempName = name;
                        t.Remove(temp);
                        tempList = t;
                    }
                }
                VCArray.Remove(tempName);
                disconnectWithClient.Enabled = true;
                sendText.Enabled = false;
                SetText("Połączenie z " + tempName + " zostało zerwane, przepustowość wynosi teraz "+ tempList.Count*2 +"Mbit/s\n");
            }
        }

        private void disconnectWithClient_Click(object sender, EventArgs e) {
            if (agent != null) {
                agent.whoToDisconnect = ((String)selectedClientBox.SelectedItem);
                agent.sendDisconnect = true;
                SetText("Wysyłam żądanie zerwania połączenia z " + ((String)selectedClientBox.SelectedItem) + "\n");
            }
            if (isConnectedToControlCloud) {
                if ((String)selectedClientBox.SelectedItem != null) {
                    String clientName = (String)selectedClientBox.SelectedItem;
                    List<int> _connidList = new List<int>();
                    NEWCONNIDARRAY.TryGetValue(lastCalledUser, out _connidList);
                    foreach (int _cid in _connidList)
                    {
                        List<String> _msgList = new List<String>();
                        _msgList.Add("REQ_DISCONN");
                        _msgList.Add(String.Empty + _cid);
                        SPacket disconPacket = new SPacket(myAddress.ToString(), new Address(1, 0, 2).ToString(), _msgList);
                        whatToSendQueue.Enqueue(disconPacket);
                    }
                    /*List<String> _msgList = new List<String>();
                    _msgList.Add("REQ_DISCONN");
                    _msgList.Add(clientName);
                    SPacket disconPacket = new SPacket(myAddress.ToString(), new Address(1, 0, 2).ToString(), _msgList);
                    whatToSendQueue.Enqueue(disconPacket);
                    sendText.Enabled = false;
                    */

                    /*if (userDict.ContainsKey(clientName)) {
                        Address _adrToDiscon;
                        userDict.TryGetValue(clientName, out _adrToDiscon);
                        int _callIDToDiscon;
                        addrCallIDDict.TryGetValue(_adrToDiscon, out _callIDToDiscon);
                        List<String> _msgList = new List<String>();
                        _msgList.Add("REQ_DISCONN");
                        _msgList.Add(String.Empty + _callIDToDiscon);
                        SPacket disconPacket = new SPacket(myAddress.ToString(), new Address(0, 0, 2).ToString(), _msgList);
                        whatToSendQueue.Enqueue(disconPacket);
                        sendText.Enabled = false;
                    }*/
                } else {
                    SetText("Nie wybrano klienta\n");
                }
            }
        }

        private void selectedClientBoxs_SelectedIndexChanged(object sender, EventArgs e) {
            //jeśli jest połączenie z tym klientem - pojawia się opcja usunięcia połączenia
            if (VCArray.ContainsKey((String)selectedClientBox.SelectedItem)) {
                disconnectWithClient.Enabled = true;
                sendText.Enabled = true;
            } else {
                disconnectWithClient.Enabled = true;
                sendText.Enabled = false;
            }
        }
        public void readConfig(String nAddr) {
            try {
                myAddress = Address.Parse(nAddr);
                isClientNumberSet = true;
                ClientNetworkNumberField.Text = String.Empty + myAddress.network;
                ClientSubnetworkNumberField.Text = String.Empty + myAddress.subnet;
                ClientHostNumberField.Text = String.Empty + myAddress.host;
                SetText("Ustalam adres klienta jako " + myAddress.ToString() + "\n");
                String path = "config" + nAddr + ".txt";
                //username = clientName;
                //usernameField. Text = clientName;
                //isClientNameSet = true;
                //SetText("Ustalam nazwę klienta jako " + username + "\n");
                //String path = "config" + clientName + ".txt";
                otherClients = new List<String>();
                using (StreamReader sr = new StreamReader(path)) {
                    string[] lines = System.IO.File.ReadAllLines(path);
                    foreach (String line in lines) {
                        String[] command = line.Split(' ');
                        if (command[0] == "ADD_CONNECTION") {
                            try {
                                if (VCArray.ContainsKey(command[1])) {
                                    List<PortVPIVCI> temp;
                                    VCArray.TryGetValue(command[1], out temp);
                                    temp.Add(new PortVPIVCI(int.Parse(command[2]), int.Parse(command[3]), int.Parse(command[4])));
                                    VCArray.Remove(command[1]);
                                    VCArray.Add(command[1], temp);
                                    SetText("Dodaję połączenie z klientem " + command[1] + " na porcie "
                                    + command[2] + " VPI " + command[3] + " VCI " + command[4] + "\n");
                                } else {
                                    List<PortVPIVCI> temp = new List<PortVPIVCI>();
                                    temp.Add(new PortVPIVCI(int.Parse(command[2]), int.Parse(command[3]), int.Parse(command[4])));
                                    VCArray.Add(command[1], temp);
                                    SetText("Dodaję połączenie z klientem " + command[1] + " na porcie "
                                    + command[2] + " VPI " + command[3] + " VCI " + command[4] + "\n");
                                }

                                if (!otherClients.Contains(command[1])) {
                                    otherClients.Add(command[1]);
                                    SetText("Dodaję klienta " + command[1] + "\n");
                                }
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
                                        List<int> _VPIList = new List<int>();
                                        for (int i = 4; i < command.Length; i++) {
                                            int vpi;
                                            if (int.TryParse(command[i], out vpi)) {
                                                _VPIList.Add(vpi);
                                            }
                                        }
                                        routeList.Add(new Route(adr, band, port, _VPIList));
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
            if (myAddress != null) {
                List<String> lines = new List<String>();
                foreach (String client in VCArray.Keys) {
                    List<PortVPIVCI> value;
                    if (VCArray.TryGetValue(client, out value)) {
                        foreach(PortVPIVCI pvv in value) {
                            lines.Add("ADD_CONNECTION " + client + " " + pvv.port + " " + pvv.VPI + " " + pvv.VCI);
                        }
                    }
                }
                foreach (String client in otherClients) {
                    if (!VCArray.ContainsKey(client)) lines.Add("ADD_CLIENT " + client);
                }
                foreach (Route rt in routeList) {
                    String _vpiString = String.Empty;
                    foreach (int _vpi in rt.VPIList) {
                        _vpiString += _vpi + " ";
                    }
                    lines.Add("ADD_ROUTE " + rt.port + " " + rt.destAddr.ToString() + " " + rt.bandwidth + " " + _vpiString);
                }
                System.IO.File.WriteAllLines("config" + myAddress.ToString() + ".txt", lines);
                SetText("Zapisuję ustawienia do pliku config" + myAddress.ToString() + ".txt\n");
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
            //if (username != null) saveConfig();
        }

        private void setClientNumber_Click(object sender, EventArgs e) {
            try {
                int clientAddressNetwork = int.Parse(ClientNetworkNumberField.Text);
                int clientAddressSubnet = int.Parse(ClientSubnetworkNumberField.Text);
                int clientAddressHost = int.Parse(ClientHostNumberField.Text);
                isClientNumberSet = true;
                myAddress = new Address(clientAddressNetwork, clientAddressSubnet, clientAddressHost);
                SetText("Adres klienta ustawiony jako " + myAddress.ToString() + "\n");
                Clientix.ActiveForm.Text = "Clientix " + myAddress.ToString();
            } catch {
                isClientNumberSet = false;
                SetText("Błędne dane wejściowe\n");
                Clientix.ActiveForm.Text = "Clientix";
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
                    controlCloudEndPoint = new IPEndPoint(controlCloudAddress, controlCloudPort);
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
                        LRM = new eLReMix(this);
                        SetText("Połączono!\n");
                        exceptionCount = 0;
                    } catch (SocketException) {
                        isConnectedToControlCloud = false;
                        SetText("Błąd podczas łączenia się z chmurą\n");
                        SetText("Złe IP lub port? Chmura nie działa?\n");
                    }
                } else {
                    SetText("Wprowadź numery sieci i podsieci\n");
                }
            } else {
                isConnectedToControlCloud = false;
                conToCloudButton.Text = "Połącz";
                SetText("Rozłączono!\n");
                if (controlCloudSocket != null) controlCloudSocket.Close();
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

                    if (receivedPacket.getParames()[0] == "OK" && receivedPacket.getSrc() == "1.0.2") {
                        isClientNameSet = true;
                        SetText("Nazwa użytkownika została zaakceptowana przez sieć\n");
                    } else if (receivedPacket.getParames()[0] == "NAME_TAKEN" && receivedPacket.getSrc() == "1.0.2") {
                        SetText("Nazwa użytkownika zajęta, wybierz inną!;");
                        username = null;
                        isClientNameSet = false;
                    } else if (receivedPacket.getParames()[0] == "CLIENTS" /*&& receivedPacket.getSrc() == "0.0.1"*/) {
                        List<string> _temp = receivedPacket.getParames();
                        _temp.Remove("CLIENTS");
                        setOtherClients(_temp);
                    } else if (receivedPacket.getParames()[0] == "YES" && receivedPacket.getSrc() == "1.0.2") {
                        try
                        {
                            SetText("Sieć zaczęła proces zestawiania połączenia z " + receivedPacket.getParames()[1] + ". Trwa oczekiwanie na akceptację drugiej strony\n");
                        }
                        catch
                        {
                            SetText("Sieć zaczęła proces zestawiania połączenia. Trwa oczekiwanie na akceptację drugiej strony\n");
                        }
                    } else if (receivedPacket.getParames()[0] == "ACK" && receivedPacket.getSrc() == "1.0.2") {
                        SetText("Użytkownik zaakceptował żądanie połączenia. Sieć zaczyna je zestawiać\n");
                    } else if (receivedPacket.getParames()[0] == "NO" && receivedPacket.getSrc() == "1.0.2") {
                        string usrToEdit = String.Empty;
                        foreach (string usr in userDict.Keys) {
                            Address _adr;
                            userDict.TryGetValue(usr, out _adr);
                            if (_adr.network == 0 && _adr.subnet == 0 && _adr.host == 0) usrToEdit = usr;
                        }
                        userDict.Remove(usrToEdit);
                        SetText("Nie masz uprawnień do wykonania takiego połączenia!\n");
                        userToBeCalled = null;
                    } else if (receivedPacket.getParames()[0] == "CONN_EST") {
                        //Address calledAddress = Address.Parse(receivedPacket.getParames()[1]);
                        SetText("Zestawiono połączenie!\n");
                        int callID = int.Parse(receivedPacket.getParames()[1]);
                        string conUsr = String.Empty;
                        foreach (Address addr in addrCallIDDict.Keys) {
                            int _cid;
                            if (addrCallIDDict.TryGetValue(addr, out _cid)) {
                                string _clientName = String.Empty;
                                foreach (Address _addr in userDict.Values) {
                                    if (_addr == addr) {
                                        _clientName = userDict.FirstOrDefault(x => x.Value.Equals(addr)).Key;
                                    }
                                }
                                foreach (PortVPIVCI pvv in AddrPortVPIVCIArray.Keys) {
                                    Address _adr;
                                    if (AddrPortVPIVCIArray.TryGetValue(pvv, out _adr)) {
                                        connectionEstablished(_clientName, pvv.port, pvv.VPI, pvv.VCI);
                                    }
                                }
                            }
                        }
                    } else if (receivedPacket.getParames()[0] == "CONN_DISCONN") {
                        SetText("Rozłączono połączenie");
                        /*int _disconCallID = int.Parse(receivedPacket.getParames()[1]);
                        Address _adrToDiscon = new Address();
                        foreach (int _callID in addrCallIDDict.Values) {
                            if (_callID == _disconCallID) {
                            _adrToDiscon = addrCallIDDict.FirstOrDefault(x => x.Value.Equals(_callID)).Key;
                            }
                        }
                        string _clientName = String.Empty;
                        foreach (Address _addr in userDict.Values) {
                            if (_addr == _adrToDiscon) {
                            _clientName = userDict.FirstOrDefault(x => x.Value.Equals(_addr)).Key;
                            }
                        }
                        List<PortVPIVCI> _valuesToBreak = new List<PortVPIVCI>();
                        VCArray.TryGetValue(_clientName, out _valuesToBreak);
                        foreach (PortVPIVCI _pvv in _valuesToBreak) {
                            connectionBroken(_pvv.port, _pvv.VPI, _pvv.VCI);
                        }
                        SetText("Rozłączono z klientem " + _clientName + "\n");*/
                    }else if (receivedPacket.getParames()[0] == "CONN_NOEST") {
                        if (lastCalledUser != null) SetText("Nie udało się nawiązać połączenia z " + lastCalledUser + "\n");
                        else SetText("Nie udało się nawiązać połączenia\n");
                    } else if (receivedPacket.getParames()[0] == "CALLING") {
                        string message = "Dzwoni " + receivedPacket.getParames()[1] + " i chce mieć prędkość " + receivedPacket.getParames()[2] + "\n" +
                                            "Akceptujesz?";
                        string caption = "RING RING";
                        var result = MessageBox.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes) {
                            List<String> _msg = new List<String>();
                            _msg.Add("ACK");
                            _msg.Add(receivedPacket.getParames()[1]);
                            _msg.Add(receivedPacket.getParames()[2]);
                            _msg.Add(receivedPacket.getParames()[3]);
                            SPacket _pck = new SPacket(myAddress.ToString(), "1.0.2", _msg);
                            whatToSendQueue.Enqueue(_pck);
                            lastCalledUser = receivedPacket.getParames()[1];
                        } else {
                            List<String> _msg = new List<String>();
                            _msg.Add("NCK");
                            _msg.Add(receivedPacket.getParames()[1]);
                            _msg.Add(receivedPacket.getParames()[2]);
                            _msg.Add(receivedPacket.getParames()[3]);
                            SPacket _pck = new SPacket(myAddress.ToString(), "1.0.2", _msg);
                            whatToSendQueue.Enqueue(_pck);
                        }
                    } else {
                        LRM.OdczytajS(receivedPacket);
                    }
                } catch {
                    SetText("WUT");
                    if (++exceptionCount == 5) {
                        this.Invoke((MethodInvoker)delegate() {
                            isConnectedToControlCloud = false;
                            conToCloudButton.Text = "Połącz";
                            SetText("Rozłączono!");
                            if (controlCloudSocket != null) controlCloudSocket.Close();
                        });
                    }
                }
            }
        }
            
        public void AddSingleEntry(Address address, int port, int vpi, int vci, int callID) {
            
            List<PortVPIVCI> _pvvList = new List<PortVPIVCI>();
            if (NEWVCARRAY.ContainsKey(lastCalledUser)) {
                NEWVCARRAY.TryGetValue(lastCalledUser, out _pvvList);
            }
            try {
                NEWVCARRAY.Remove(lastCalledUser);
            } catch { }
            _pvvList.Add(new PortVPIVCI(port, vpi, vci));
            NEWVCARRAY.Add(lastCalledUser, _pvvList);

            List<int> _connidList = new List<int>();
            if (NEWCONNIDARRAY.ContainsKey(lastCalledUser))
            {
                NEWCONNIDARRAY.TryGetValue(lastCalledUser, out _connidList);
            }
            try
            {
                NEWCONNIDARRAY.Remove(lastCalledUser);
            }
            catch { }
            _connidList.Add(callID);
            NEWCONNIDARRAY.Add(lastCalledUser, _connidList);

            AddrPortVPIVCIArray.Add(new PortVPIVCI(port, vpi, vci), address);
            SetText("Dodaję wpis w tablicy VCArray na port " + port + " VPI " + vpi + " VCI " + vci + "\n");
            if (lastCalledAddress != null) {
                if (addrCallIDDict.ContainsKey(lastCalledAddress)) {
                    addrCallIDDict.Remove(lastCalledAddress);
                    addrCallIDDict.Add(lastCalledAddress, callID);
                } else addrCallIDDict.Add(lastCalledAddress, callID);
            }
            String _str;
            if (lastCalledUser != null) _str = "[ " + port + " , " + vpi + " , " + vci + " ] : " + lastCalledUser;
            else _str = "[ " + port + " , " + vpi + " , " + vci + " ]";
            howToSendDict.Add(_str, new PortVPIVCI(port, vpi, vci));
            BindingSource bs = new BindingSource();
            bs.DataSource = howToSendDict.Keys;
            this.Invoke((MethodInvoker)delegate() {
                howToSendComboBox.DataSource = bs;
                sendText.Enabled = true;
            });
        }

        public void RemoveSingleEntry(Address address, int port, int vpi, int vci, int callID) {
            AddrPortVPIVCIArray.Remove(new PortVPIVCI(port, vpi, vci));
            SetText("Usuwam wpis w tablicy VCArray: port " + port + " VPI " + vpi + " VCI " + vci + "\n");
            String _str = "[ " + port + " , " + vpi + " , " + vci + " ]";
            string _strToDelete = String.Empty;
            foreach (string _shown in howToSendDict.Keys)
            {
                if (_shown.Contains(_str))
                {
                    _strToDelete = _shown;
                }
            }
            try
            {
                howToSendDict.Remove(_strToDelete);
                BindingSource bs = new BindingSource();
                bs.DataSource = howToSendDict.Keys;
                this.Invoke((MethodInvoker)delegate()
                {
                    howToSendComboBox.DataSource = bs;
                });
            }
            catch
            {
                SetText("Nie usunąłem wpisu z comboBoxa :<\n");
            }
        }
        /// <summary>
        /// wątek wysyłający wiadomości do chmury
        /// </summary>
        public void controlSender() {
            while (isConnectedToControlCloud) {
                //jeśli coś jest w kolejce - zdejmij i wyślij
                if (whatToSendQueue.Count != 0) {
                    SPacket _pck = (SPacket)whatToSendQueue.Dequeue();
                    BinaryFormatter bformatter = new BinaryFormatter();
                    bformatter.Serialize(controlNetworkStream, _pck);
                    controlNetworkStream.Flush();
                    String[] _argsToShow = _pck.getParames().ToArray();
                    String argsToShow = "";
                    foreach (String str in _argsToShow) {
                        argsToShow += str + " ";
                    }
                    SetText("Wysłano: " + _pck.getSrc() + ":" + _pck.getDest() + ":" + argsToShow + "\n");
                }
            }
        }

        private void Clientix_Load(object sender, EventArgs e) {
            
        }

        private void chooseTextFile_Click(object sender, EventArgs e) {
            string path;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK) {
                path = openFileDialog.FileName;
                readConfig(path, true);
            }
        }

        public void readConfig(String path, bool justToOverload) {
            try {
                //myAddress = Address.Parse(nAddr);
                //isNodeAddressSet = true;
                //NodeNetworkNumberField.Text = String.Empty + myAddress.network;
                //NodeSubnetworkNumberField.Text = String.Empty + myAddress.subnet;
                //NodeHostNumberField.Text = String.Empty + myAddress.host;
                SetText("Wczytuje plik konfiguracyjny z " + path + "\n");
                //String path = "config" + nAddr + ".txt";
                using (StreamReader sr = new StreamReader(path)) {
                    string[] lines = System.IO.File.ReadAllLines(path);
                    foreach (String line in lines) {
                        String[] command = line.Split(' ');
                        if (command[0] == "ADD_CONNECTION") {
                            try {
                                if (VCArray.ContainsKey(command[1])) {
                                    List<PortVPIVCI> temp;
                                    VCArray.TryGetValue(command[1], out temp);
                                    temp.Add(new PortVPIVCI(int.Parse(command[2]), int.Parse(command[3]), int.Parse(command[4])));
                                    VCArray.Remove(command[1]);
                                    VCArray.Add(command[1], temp);
                                    SetText("Dodaję połączenie z klientem " + command[1] + " na porcie "
                                    + command[2] + " VPI " + command[3] + " VCI " + command[4] + "\n");
                                } else {
                                    List<PortVPIVCI> temp = new List<PortVPIVCI>();
                                    temp.Add(new PortVPIVCI(int.Parse(command[2]), int.Parse(command[3]), int.Parse(command[4])));
                                    VCArray.Add(command[1], temp);
                                    SetText("Dodaję połączenie z klientem " + command[1] + " na porcie "
                                    + command[2] + " VPI " + command[3] + " VCI " + command[4] + "\n");
                                }

                                if (!otherClients.Contains(command[1])) {
                                    otherClients.Add(command[1]);
                                    SetText("Dodaję klienta " + command[1] + "\n");
                                }
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
                                        List<int> _VPIList = new List<int>();
                                        for (int i = 4; i < command.Length; i++) {
                                            int vpi;
                                            if (int.TryParse(command[i], out vpi)) {
                                                _VPIList.Add(vpi);
                                            }
                                        }
                                        routeList.Add(new Route(adr, band, port, _VPIList));
                                    } else SetText("Zły format danych\n");
                                } else SetText("Zły format danych\n");
                            } else SetText("Zły format danych\n");
                        } else if (command[0] == "SET_ADDR") {
                            try {
                                myAddress = new Address(int.Parse(command[1]), int.Parse(command[2]), int.Parse(command[3]));
                                isClientNumberSet = true;
                                ClientNetworkNumberField.Text = String.Empty + myAddress.network;
                                ClientSubnetworkNumberField.Text = String.Empty + myAddress.subnet;
                                ClientHostNumberField.Text = String.Empty + myAddress.host;
                                SetText("Ustalam adres klienta jako " + myAddress.ToString() + "\n");
                            } catch {
                                SetText("komenda ustalenia adresu została niepoprawnie sformułowana");
                            }
                        }
                    }
                }
            } catch (Exception exc) {
                SetText("Błąd podczas konfigurowania pliku konfiguracyjnego\n");
                SetText(exc.Message + "\n");
            }
        }

        private void Clientix_MouseMove(object sender, MouseEventArgs e)
        {
            if (myAddress != null && isNameSet != true)
            {
                Clientix.ActiveForm.Text = "Clientix " + myAddress.ToString();
                isNameSet = true;
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
