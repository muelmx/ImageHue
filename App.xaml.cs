using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Autofac.Core;

namespace ImageHue
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Bootstrapper.Start();

            var window = new MainWindow
            {
                DataContext = Bootstrapper.RootVisual.Main
            };

            window.Closed += (s, a) =>
            {
                Bootstrapper.Stop();
            };

            window.Show();
        }
    }
}
