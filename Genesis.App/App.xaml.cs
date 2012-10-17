using Genesis.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Genesis.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            var l = this.Resources["Locator"] as Locator;
                l.Cleanup();
        }
    }
}
