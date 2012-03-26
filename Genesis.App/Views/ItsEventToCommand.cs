using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.Command;

namespace Genesis.Command
{
    public class ItsEventToCommand : EventToCommand
    {
        protected override void Invoke(object parameter)
        {
            var args = parameter as RoutedEventArgs;
            if (args != null)
            {
                if (args.OriginalSource != this.AssociatedObject)
                    return;
            }
            base.Invoke(parameter);
        }
    }
}
