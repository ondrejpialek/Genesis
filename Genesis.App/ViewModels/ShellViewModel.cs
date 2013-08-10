using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Genesis.Views;
using System.Linq;

namespace Genesis.ViewModels
{
    public interface IShellViewModel {}

    public class ShellViewModel : Conductor<ISectionViewModel>.Collection.AllActive, IShellViewModel
    {
        public ShellViewModel(IEnumerable<ISectionViewModel> sectionViewModels)
        {
            DisplayName = "Genesis";

            Items.AddRange(sectionViewModels);

            /*
                = new ObservableCollection<SectionViewModel>() {
                new SectionViewModel("Localities", () => new Genesis.Views.Data()),
                new SectionViewModel("Map", () => new Map()),
                new SectionViewModel("Mice", () => new Mice()),
                new SectionViewModel("Frequencies", () => new Analyze()),
                new SectionViewModel("Fst", () => new Fst()),
                new SectionViewModel("Genetics", () => new Settings()),
                new SectionViewModel("Import", () => new Import()),
            };       */
        }

        protected override void OnActivate()
        {
            ActivateItem(Items.First(i => i is MapSectionViewModel));
        }
    }

    public interface ISectionViewModel : IScreen
    {
    }
}