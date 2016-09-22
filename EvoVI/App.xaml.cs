using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EvoVIConfigurator
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        #region Events
        private void OnAppStartup(object sender, StartupEventArgs e)
        {
            // Set command line arguments; pass them to the configuration manager
            foreach(string arg in e.Args)
            {
                switch(arg)
                {
                    case "ignoregamestart":
                        EvoVI.ConfigurationManager.StartupParams.IgnoreGameProcessStart = true;
                        break;
                    case "noidletimeout":
                        EvoVI.ConfigurationManager.StartupParams.NoIdleTimeout = true;
                        break;
                }
            }
        }
        #endregion
    }
}
