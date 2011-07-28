using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Daemonized.TurnOut
{
    [ProgId("TurnOut.NET.Url")]
    public class StartupClass
    {
        
        public StartupClass() { }
        [STAThread]
        /// 
        public static void Main(string[] arguments)
        {
            App myApp = new App();
            MainWindow win = new MainWindow();

            if (arguments.Length > 0)
                win.TargetUrl = new Uri(arguments[0]);

            if (!win.IsSingle())
                Environment.Exit(0);

            Environment.ExitCode = myApp.Run(win);
        }        
    }
}
