using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Clientix {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args) {
            String clientName;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Clientix c = new Clientix();
            if (args != null) {
                try {
                    clientName = args[0];
                    c.readConfig(clientName);
                    c.connect = true;
                } catch {}
            } 
            Application.Run(c);
        }
    }
}
