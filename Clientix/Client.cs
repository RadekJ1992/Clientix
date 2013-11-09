using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Clientix {

    public partial class Client : Form {

        //otrzymany i wysyłany pakiets
        private Packet.ATMPacket receivedPacket;
        private Packet.ATMPacket processedPacket;

        //unikalna nazwa klienta widziana przez zarządcę
        private String clientName;

        //tablica innych węzłów klienckich podłączonych do zarządcy
        private String[] otherClients;

        // tablica kierowania
        private Dictionary<Packet.VpiVci, Packet.VpiVci> VCArray;

        public Client() {         
            InitializeComponent();
        }

        private void sendMessage(object sender, EventArgs e) {

        }

        private void chooseServerIPAndPort(object sender, EventArgs e) {

        }

        private void chooseManagerIPAndPort(object sender, EventArgs e) {

        }
    }
}
