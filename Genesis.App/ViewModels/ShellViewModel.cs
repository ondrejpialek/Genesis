using System.Collections.ObjectModel;
using Caliburn.Micro;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Genesis.Views;

namespace Genesis.ViewModels
{
    public interface IShellViewModel {}

    public class ShellViewModel : Screen, IShellViewModel
    {
        public ObservableCollection<SectionViewModel> Items { get; protected set; }

        public ShellViewModel()
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

        private SectionViewModel selectedSection;
        public SectionViewModel SelectedSection
        {
            get
            {
                return selectedSection;
            }
            set
            {
                this.Set(() => SelectedSection, ref selectedSection, value);
            }
        }

        private RelayCommand refresh;
        public new RelayCommand Refresh
        {
            get
            {
                return refresh ?? (refresh = new RelayCommand(() =>
                {
                    Messenger.Default.Send(new GenericMessage<Message>(this, SelectedSection.Content.DataContext, Message.Refresh));
                }));
            }
        }
    }
}