using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Server;

namespace Ludavi
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // On start stuff here
            base.OnStartup(e);

            ServerLogic.RunServer();

            // Or here, where you find it more appropriate
        }
    }
}
