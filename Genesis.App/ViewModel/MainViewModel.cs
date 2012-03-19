using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using Genesis.Views;
using GalaSoft.MvvmLight.Command;

namespace Genesis.ViewModel
{

    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<SectionViewModel> Items { get; protected set; }

        public MainViewModel()
        {
            Items = new ObservableCollection<SectionViewModel>() {
                new SectionViewModel("Localities", () => new Data()),
                new SectionViewModel("Map", () => new Map()),
                new SectionViewModel("Mice", () => new Mice()),
                new SectionViewModel("Analysis", () => new Analyze()),
                new SectionViewModel("Genetics", () => new Settings()),
                new SectionViewModel("Import", () => new Import()),
            };
        }
    }
}