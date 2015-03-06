using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LyncWPFApplication3
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
        }

        [STAThread]
        static void Main()
        {
            using (Mutex mutex = new Mutex(false, "ccLyncLyte"))
            {
                if (!mutex.WaitOne(TimeSpan.Zero, false))
                {
                    // Already running
                    return;
                }

                App app = new App();
                NiceConfig niceConfig = new NiceConfig();
                
                app.Run(niceConfig);
            }  
        }
    }
}
