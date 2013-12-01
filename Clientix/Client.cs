﻿using Packet;
using System;
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

        //fuck delegates!
        delegate void SetTextCallback(string text);

        //otrzymany i wysyłany pakiets
        private Packet.ATMPacket receivedPacket;
        private Packet.ATMPacket processedPacket;

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

        private Socket cloudSocket;
        private Socket managerSocket;

        //nazwa klienta
        private String username;

        private Thread receiveThread;     //wątek służący do odbierania połączeń
        private Thread sendThread;        // analogicznie - do wysyłania

        // do odbierania
        private NetworkStream networkStream;
        //do wysyłania
        private NetworkStream netStream;

        public bool isRunning { get; private set; }     //info czy klient chodzi - dla zarządcy

        private bool isClientNameSet;
        public bool isConnectedToCloud { get; private set; } // czy połączony z chmurą?
        public bool isConnectedToManager { get; private set; } // czy połączony z zarządcą?

        //tablica innych węzłów klienckich podłączonych do sieci otrzymana do zarządcy
        private List<String> otherClients;

        //słownik klientów, z którymi mamy połączenie i odpowiadających im komvinacji port,vpi,vci
        private Dictionary<String, PortVPIVCI> VCArray;

        public Clientix() {
            InitializeComponent();

            //tooltip dla nazwy klienta
            System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();
            toolTip.SetToolTip(this.label7, "Nazwa klienta może zawierać litery, cyfry i znak '_'");
            toolTip.SetToolTip(this.usernameField, "Nazwa klienta może zawierać litery, cyfry i znak '_'");
            toolTip.AutoPopDelay = 2000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 500;
            toolTip.ShowAlways = true;

            isClientNameSet = false;
            otherClients = new List<string>();
            VCArray = new Dictionary<String, PortVPIVCI>();
            selectedClientBox.DataSource = otherClients;
        }

        private void sendMessage(object sender, EventArgs e) {
            packetsFromString = Packet.AAL.getATMPackets(enteredTextField.Text);
            if (!isConnectedToCloud) log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") +
                                                    " Not connected to cloud!");
            else {
                foreach (Packet.ATMPacket packet in packetsFromString) {
                    Boolean isPortVPIVCISet = false;
                    try {
                        packet.port = int.Parse(outPortTextBox.Text);
                        packet.VPI = int.Parse(outVPITextBox.Text);
                        packet.VCI = int.Parse(outVCITextBox.Text);
                        isPortVPIVCISet = true;
                    }
                    catch {
                        isPortVPIVCISet = false;
                    }
                    netStream = new NetworkStream(cloudSocket);
                    PortVPIVCI temp;
                    if (VCArray.TryGetValue((String)selectedClientBox.SelectedItem, out temp)) {
                        if (isPortVPIVCISet) {
                            SetText("Wysyłam pakiet do " + (String)selectedClientBox.SelectedItem + " z ustawieniem [" + int.Parse(outPortTextBox.Text) + ";" +
                                            int.Parse(outVPITextBox.Text) + ";" + int.Parse(outVCITextBox.Text) + "]\n");
                        }
                        else {
                            packet.port = temp.port;
                            packet.VPI = temp.VPI;
                            packet.VCI = temp.VCI;
                            SetText("Wysyłam pakiet do " + (String)selectedClientBox.SelectedItem + " z ustawieniem [" + packet.port + ";" +
                                           packet.VPI + ";" + packet.VCI + "]\n");
                        }
                        BinaryFormatter bformatter = new BinaryFormatter();
                        bformatter.Serialize(netStream, packet);
                        netStream.Close();
                    }
                }
            }
            enteredTextField.Clear();
            outPortTextBox.Clear();
            outVCITextBox.Clear();
            outVPITextBox.Clear();
        }

        private void sendMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            //jeśli naciśniesz enter; żeby to działało to wystarczy jeszcze tylko podlinkować zdażenie pod pole tekstowe (zrobione)
            if (sendText.Enabled && e.KeyChar.Equals((char)Keys.Enter)) sendMessage(sender, e);
        }

        private void connectToCloud(object sender, EventArgs e) {
            if (isClientNameSet) {
                if (IPAddress.TryParse(cloudIPField.Text, out cloudAddress)) {
                    log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " >Cloud IP set properly as " + cloudAddress.ToString() + " \n");
                } else {
                    log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " >Error reading cloud IP" + " \n");
                }
                if (Int32.TryParse(cloudPortField.Text, out cloudPort)) {
                    log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " >Cloud port set properly as " + cloudPort.ToString() + " \n");
                } else {
                    log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " >Error reading cloud Port" + " \n");
                }

                cloudSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                cloudEndPoint = new IPEndPoint(cloudAddress, cloudPort);
                try {
                    cloudSocket.Connect(cloudEndPoint);
                    isConnectedToCloud = true;
                    receiveThread = new Thread(this.receiver);
                    receiveThread.IsBackground = true;
                    receiveThread.Start();
                } catch (SocketException ex) {
                    isConnectedToCloud = false;
                    log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " >Error while connecting to cloud\n");
                    log.AppendText("Wrong IP or port?\n");
                }
            } else SetText("Ustaw nazwę klienta!\n");
        }

        private void connectToManager(object sender, EventArgs e) {
            if (isClientNameSet) {
                if (IPAddress.TryParse(managerIPField.Text, out managerAddress)) {
                    log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " >Manager IP set properly as " + managerAddress.ToString() + " \n");
                } else {
                    log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " >Error reading manager IP" + " \n");
                }
                if (Int32.TryParse(managerPortField.Text, out managerPort)) {
                    log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " >Manager port set properly as " + managerPort.ToString() + " \n");
                } else {
                    log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " >Error reading manager Port" + " \n");
                }

                managerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                managerEndPoint = new IPEndPoint(managerAddress, managerPort);
                try {
                    managerSocket.Connect(managerEndPoint);
                    isConnectedToManager = true;

                    //działanie AGENTA


                } catch (SocketException ex) {
                    isConnectedToManager = false;
                    log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " >Error while connecting to manager\n");
                    log.AppendText("Wrong IP or port?\n");
                }
            } else SetText("Ustal nazwę klienta!\n");   
        }

        private void receiver() {
            try {
                if (networkStream == null) {
                    networkStream = new NetworkStream(cloudSocket);
                    //tworzy string 'client ' i tu jego nazwę
                    String welcomeString = "Client " + username;
                    //tworzy tablicę bajtów z tego stringa
                    byte[] welcomeStringBytes = AAL.GetBytesFromString(welcomeString);
                    //wysyła tą tablicę bajtów streamem
                    networkStream.Write(welcomeStringBytes, 0, welcomeStringBytes.Length);
                }
                BinaryFormatter bf = new BinaryFormatter();
                receivedPacket = (Packet.ATMPacket)bf.Deserialize(networkStream);
                int tempSeq = 0;
                int tempMid = 0;
                // gdy wiadomość zawarta jest w jednym pakiecie
                if (receivedPacket.PacketType == Packet.ATMPacket.AALType.SSM) {
                    SetText(Packet.AAL.getStringFromPacket(receivedPacket) + "\n");
               }
                else if (receivedPacket.PacketType == Packet.ATMPacket.AALType.BOM) {
                    tempSeq = 0;
                    tempMid = receivedPacket.AALMid;
                    queuedReceivedPackets.Clear();
                    queuedReceivedPackets.Enqueue(receivedPacket);
                }
                else if (receivedPacket.PacketType == Packet.ATMPacket.AALType.COM) {
                    if (receivedPacket.AALMid == tempMid) {
                        //sprawdza kolejnosc AALSeq
                        if (receivedPacket.AALSeq == ++tempSeq) {
                            queuedReceivedPackets.Enqueue(receivedPacket);
                        }
                        else {
                            SetText("Packet lost! :<");
                        }
                    }
                    else {
                        SetText("packet from another message (different AALMid)");
                    }
                }
                else if (receivedPacket.PacketType == Packet.ATMPacket.AALType.EOM) {
                    queuedReceivedPackets.Enqueue(receivedPacket);
                    SetText(Packet.AAL.getStringFromPackets(queuedReceivedPackets));
                    queuedReceivedPackets.Clear();
                    tempSeq = 0;
                    tempMid = 0;
                }
                //networkStream.Close();
            } catch (SerializationException e) {
                //gdy przyjdzie coś, czego nie da się zserializować - nie rób nic
            }
            receiver();
        }

        private void SetText(string text) {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (this.log.InvokeRequired) {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else {
                this.log.AppendText(text);
            }
        }

        private void setUsernameButton_Click(object sender, EventArgs e) {
            if (!usernameField.Text.Equals("")) {
                if (Regex.IsMatch(usernameField.Text, "^[a-zA-Z0-9 _]+$")) {
                    username = usernameField.Text;
                    isClientNameSet = true;
                    SetText("Nazwa klienta ustawiona jako " + username + "\n");
                } else this.SetText("Dawaj jakąś ludzką nazwę (dozwolone tylko litery, cyfry i znak '_')\n");
            } else {
                SetText("Dawaj jakąś ludzką nazwę (dozwolone tylko litery, cyfry i znak '_')\n");
                isClientNameSet = false;
            }
        }

        private void getOtherClients_Click(object sender, EventArgs e) {
            //TU AGENT POBIERA OD ZARZADCY NAZWY KLIENTOW
            //dla przykladu
            otherClients.Add("Mietek");
            otherClients.Add("Zenek");
            BindingSource bs = new BindingSource();
            bs.DataSource = otherClients;
            selectedClientBox.DataSource = bs;
        }

        private void connectWithClientButton_Click(object sender, EventArgs e) {
            if ((String)selectedClientBox.SelectedItem != null) {
                String clientName = (String)selectedClientBox.SelectedItem;
                //TU AGENT KRZYCZY : "DAWAJ Z MIETKIEM!"
                //tu ustalic by jak polaczenie zostanie ustanowione to by ustawialo button 'disconnect' na 'enabled = true'
                disconnectWithClient.Enabled = true;
                sendText.Enabled = true;
                //i dopisać nazwe uzytkownika do tablicy connectedClients
                /* to jest przykładowa wartość */
                PortVPIVCI temp = new PortVPIVCI(4, 3, 2);
                VCArray.Add(clientName, temp);
            } else {
                SetText("Nie wybrano klienta");
            }
        }

        private void disconnectWithClient_Click(object sender, EventArgs e) {
            //TU AGENT KRZYCZY : "ROZLACZ Z MIETKIEM!"
            disconnectWithClient.Enabled = false;
            sendText.Enabled = false;
            VCArray.Remove((String)selectedClientBox.SelectedItem);
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

    }
}
