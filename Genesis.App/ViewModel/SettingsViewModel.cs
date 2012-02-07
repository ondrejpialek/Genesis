using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

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

            Chromosomes = new ObservableCollection<Chromosome>();
            foreach (var chromosome in context.Chromosomes)
            {
                Chromosomes.Add(chromosome);
            }
        }

        public ObservableCollection<Chromosome> Chromosomes
        {
            get;
            protected set;
        }
    }
}