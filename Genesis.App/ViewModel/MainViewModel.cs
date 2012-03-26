using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using Genesis.Views;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace Genesis.ViewModel
{

    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<SectionViewModel> Items { get; protected set; }

        public MainViewModel()
        {
            Items = new ObservableCollection<SectionViewModel>() {
                new SectionViewModel("Localities", () => new Genesis.Views.Data()),
                new SectionViewModel("Map", () => new Map()),
                new SectionViewModel("Mice", () => new Mice()),
                new SectionViewModel("Frequencies", () => new Analyze()),
                new SectionViewModel("Fst", () => new Fst()),
                new SectionViewModel("Genetics", () => new Settings()),
                new SectionViewModel("Import", () => new Import()),
            };
        }

        private RelayCommand refresh;
        public RelayCommand Refresh
        {
            get
            {
                return refresh ?? (refresh = new RelayCommand(() =>
                {
                    Messenger.Default.Send(Message.Refresh);
                }));
            }
        }
    }
}