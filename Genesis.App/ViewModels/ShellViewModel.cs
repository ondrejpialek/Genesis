using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Genesis.Views;
using System.Linq;

namespace Genesis.ViewModels
{
    public interface IShellViewModel {}

    public class ShellViewModel : Conductor<ISectionViewModel>.Collection.OneActive, IShellViewModel
    {
        public ShellViewModel(IEnumerable<ISectionViewModel> sectionViewModels)
        {
            DisplayName = "Genesis";
            Items.AddRange(sectionViewModels.OrderBy(s => s.Order).ToList());
        }

        protected override void OnActivate()
        {
            ActivateItem(Items.First(i => i is MiceSectionViewModel));
        }
    }

    public interface ISectionViewModel : IScreen
    {
        int Order { get; }
    }
}