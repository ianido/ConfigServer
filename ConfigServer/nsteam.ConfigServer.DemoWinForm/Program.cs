using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace nsteam.ConfigServer.DemoWinForm
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);



            IConfigManagement config = new ConfigManagementWithClient();


            Application.Run(new Form1(config));
        }
    }
}
