using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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

        private RelayCommand<Chromosome> addTrait;
        public RelayCommand<Chromosome> AddTrait
        {
            get
            {
                return addTrait ?? (addTrait = new RelayCommand<Chromosome>((c) =>
                {
                    c.Genes.Add(new Gene()
                    {
                        Name = "NEW GENE"
                    });
                }));
            }
        }

        private RelayCommand<Gene> removeTrait;
        public RelayCommand<Gene> RemoveTrait
        {
            get
            {
                return removeTrait ?? (removeTrait = new RelayCommand<Gene>((g) =>
                {
                    g.Chromosome.Genes.Remove(g);
                    context.Genes.Remove(g);
                }));
            }
        }

        private RelayCommand<Gene> addAllele;
        public RelayCommand<Gene> AddAllele
        {
            get
            {
                return addAllele ?? (addAllele = new RelayCommand<Gene>((g) =>
                {
                    g.Alleles.Add(new Allele()
                    {
                        Value = "NEW ALLELE"
                    });
                }));
            }
        }

        private RelayCommand<Allele> removeAllele;
        public RelayCommand<Allele> RemoveAllele
        {
            get
            {
                return removeAllele ?? (removeAllele = new RelayCommand<Allele>(
                a =>
                {
                    context.Alleles.Remove(a);                                
                }));
            }
        }

        private RelayCommand addSpecies;
        public RelayCommand AddSpecies
        {
            get
            {
                return addSpecies ?? (addSpecies = new RelayCommand(() =>
                {
                    context.Species.Add(new Species()
                    {
                        Name = "NEW SPECIES"
                    });
                }));
            }
        }

        private RelayCommand<Species> removeSpecies;
        public RelayCommand<Species> RemoveSpecies
        {
            get
            {
                return removeSpecies ?? (removeSpecies = new RelayCommand<Species>((s) =>
                {
                    context.Species.Remove(s);
                }));
            }
        }

        private RelayCommand save;
        public RelayCommand Save
        {
            get
            {
                if (save == null)
                {
                    save = new RelayCommand(() =>
                    {
                        context.SaveChanges();
                    });
                }

                return save;
            }
        }
    }
}