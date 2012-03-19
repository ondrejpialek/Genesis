using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using System.Data.Entity;

namespace Genesis.ViewModel
{

    public class SettingsViewModel : ViewModelBase
    {
        private GenesisContext context;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public SettingsViewModel()
        {
            context = new GenesisContext();
            context.Chromosomes.Load();
            context.Species.Load();
        }

        public ObservableCollection<Chromosome> Chromosomes
        {
            get
            {
                return context.Chromosomes.Local;
            }
        }

        public ObservableCollection<Species> Species
        {
            get
            {
                return context.Species.Local;
            }
        }
    }
}