using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
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

        private Thread receiveThread;     //wątek służący do odbierania połączeń
        private Thread sendThread;        // analogicznie - do wysyłania

        private NetworkStream netStream;

        public bool isRunning { get; private set; }     //info czy klient chodzi - dla zarządcy

        public bool isConnectedToCloud { get; private set; } // czy połączony z chmurą?
        public bool isConnectedToManager { get; private set; } // czy połączony z zarządcą?

        //unikalna nazwa klienta widziana przez zarządcę
        private String clientName;

        //tablica innych węzłów klienckich podłączonych otrzymana do zarządcy
        private String[] otherClients;

        public Clientix() {
            InitializeComponent();
        }

        private void sendMessage(object sender, EventArgs e) {
            packetsFromString = Packet.AAL.getATMPackets(enteredTextField.Text);
            if (!isConnectedToCloud) log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") +
                                                    " Not connected to cloud!");
            else {
                foreach (Packet.ATMPacket packet in packetsFromString) {
                    netStream = new NetworkStream(cloudSocket);
                    //:DLA_PRZYKŁADU
                    packet.VCI = 1;
                    packet.VPI = 1;
                    packet.port = 1;
                    //
                    log.AppendText("wysyłam pakiet!\n");
                    BinaryFormatter bformatter = new BinaryFormatter();
                    bformatter.Serialize(netStream, packet);
                    netStream.Close();

                }
            }
        }

        private void connectToCloud(object sender, EventArgs e) {
            if (IPAddress.TryParse(cloudIPField.Text, out cloudAddress)) {
                log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " >Cloud IP set properly as " + cloudAddress.ToString() + " \n");
            }
            else {
                log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " >Error reading cloud IP" + " \n");
            }
            if (Int32.TryParse(cloudPortField.Text, out cloudPort)) {
                log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " >Cloud port set properly as " + cloudPort.ToString() + " \n");
            }
            else {
                log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " >Error reading cloud Port" + " \n");
            }

            cloudSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            cloudEndPoint = new IPEndPoint(cloudAddress, cloudPort);
            try {
                cloudSocket.Connect(cloudEndPoint);
                isConnectedToCloud = true;
                receiveThread = new Thread(this.receiver);
                receiveThread.Start();
            }
            catch (SocketException ex) {
                isConnectedToCloud = false;
                log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " >Error while connecting to cloud\n");
                log.AppendText("Wrong IP or port?\n");
            }
        }

        private void connectToManager(object sender, EventArgs e) {
            if (IPAddress.TryParse(managerIPField.Text, out managerAddress)) {
                log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " >Manager IP set properly as " + managerAddress.ToString() + " \n");
            }
            else {
                log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " >Error reading manager IP" + " \n");
            }
            if (Int32.TryParse(managerPortField.Text, out managerPort)) {
                log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " >Manager port set properly as " + managerPort.ToString() + " \n");
            }
            else {
                log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " >Error reading manager Port" + " \n");
            }

            managerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            managerEndPoint = new IPEndPoint(managerAddress, managerPort);
            try {
                managerSocket.Connect(managerEndPoint);
                isConnectedToManager = true;

                //działanie AGENTA


            }
            catch (SocketException ex) {
                isConnectedToManager = false;
                log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " >Error while connecting to manager\n");
                log.AppendText("Wrong IP or port?\n");
            }
            
        }

        private void receiver() {
            NetworkStream networkStream = new NetworkStream(cloudSocket);
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
            networkStream.Close();
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

    }
}
