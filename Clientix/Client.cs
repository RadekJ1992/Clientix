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

    public partial class Client : Form {

        private struct VPIVCI {
            public int VPI;
            public int VCI;
        }

        //otrzymany i wysyłany pakiets
        private Packet.ATMPacket receivedPacket;
        private Packet.ATMPacket processedPacket;

        private Queue<Packet.ATMPacket> packetsFromString;

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

        public bool isRunning { get; private set; }     //info czy klient chodzi - dla zarządcy

        public bool isConnectedToCloud { get; private set; } // czy połączony z chmurą?
        public bool isConnectedToManager { get; private set; } // czy połączony z zarządcą?
        
        //unikalna nazwa klienta widziana przez zarządcę
        private String clientName;

        //tablica innych węzłów klienckich podłączonych otrzymana do zarządcy
        private String[] otherClients;

        // tablica kierowania
        private Dictionary<VPIVCI,VPIVCI> VCArray;

        public Client() {
            InitializeComponent();
        }

        private void sendMessage(object sender, EventArgs e) {
            packetsFromString = Packet.AAL.getATMPackets(enteredTextField.Text);
            if (!isConnectedToCloud) log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") +
                                                    " Not connected to cloud!");
            else {
                foreach (Packet.ATMPacket packet in packetsFromString) {
                    //:DLA_PRZYKŁADU
                    packet.VCI = 1;
                    packet.VPI = 1;
                    packet.port = 1;
                    //
                    log.AppendText("wysyłam pakiet!");
                    Stream stream = new NetworkStream(cloudSocket);
                    BinaryFormatter bformatter = new BinaryFormatter();
                    bformatter.Serialize(stream, packet);
                    stream.Close();
                }
            }
        }
        
        private void connectToCloud(object sender, EventArgs e) {
            if (IPAddress.TryParse(cloudIPField.Text, out cloudAddress)) {
                log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " Cloud IP set properly as " + cloudAddress.ToString() + " \n");
            }
            else {
                log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " Error reading cloud IP" + " \n");
            }
            if (Int32.TryParse(cloudPortField.Text, out cloudPort)) {
                log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " Cloud port set properly as " + cloudPort.ToString() + " \n");
            }
            else {
                log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " Error reading cloud Port" + " \n");
            }

            cloudSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint endPoint = new IPEndPoint(cloudAddress, cloudPort);
            try {
                cloudSocket.Connect(endPoint);
                isConnectedToCloud = true;
            } catch (SocketException ex) {
                isConnectedToCloud = false;
                log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " Error while connecting to cloud\n");
                log.AppendText(DateTime.Now.ToString("Wrong IP or port?\n"));
            }
        }
        
        private void connectToManager(object sender, EventArgs e) {
            if (IPAddress.TryParse(managerIPField.Text, out managerAddress)) {
                log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " Manager IP set properly as " + managerAddress.ToString() + " \n");
            }
            else {
                log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " Error reading manager IP" + " \n");
            }
            if (Int32.TryParse(managerPortField.Text, out managerPort)) {
                log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " Manager port set properly as " + managerPort.ToString() + " \n");
            }
            else {
                log.AppendText(DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt") + " Error reading manager Port" + " \n");
            }
        }       
    }
}
