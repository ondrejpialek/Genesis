using System.Windows;
using Genesis.ViewModels;

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
