using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Data.Entity;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;
using System.Data.Entity.Validation;
using System;

namespace Genesis.ViewModel
{

    public class SettingsViewModel : ViewModelBase
    {

        #region tree view models

        public class AlleleViewModel : ViewModelBase
        {
            private Allele a;

            public string Value
            {
                get
                {
                    return a.Value;
                }
                set
                {
                    RaisePropertyChanging(() => Value);
                    a.Value = value;
                    RaisePropertyChanged(() => Value);
                }
            }

            public Species Species
            {
                get
                {
                    return a.Species;
                }
                set
                {
                    RaisePropertyChanging(() => Species);
                    a.Species = value;
                    RaisePropertyChanged(() => Species);
                }
            }

            public AlleleViewModel(Allele a)
            {
                this.a = a;
            }

            public Allele GetAllele()
            {
                return a;
            }
        }

        public class GeneViewModel : ViewModelBase
        {
            private GenesisContext context;
            private Gene gene;

            public string Name
            {
                get
                {
                    return gene.Name;
                }
                set
                {
                    RaisePropertyChanging(() => Name);
                    gene.Name = value;
                    RaisePropertyChanged(() => Name);
                }
            }

            public ObservableCollection<AlleleViewModel> Alleles { get; protected set; }

            public GeneViewModel(Gene g, GenesisContext context)
            {
                this.gene = g;
                this.context = context;
                Alleles = new ObservableCollection<AlleleViewModel>(g.Alleles.Select(a => new AlleleViewModel(a)));
            }

            private RelayCommand addAllele;
            public RelayCommand AddAllele
            {
                get
                {
                    return addAllele ?? (addAllele = new RelayCommand(() =>
                    {
                        var allele = new Allele()
                        {
                            Value = "NEW ALLELE"
                        };
                        gene.Alleles.Add(allele);
                        Alleles.Add(new AlleleViewModel(allele));
                    }));
                }
            }

            private RelayCommand<AlleleViewModel> removeAllele;
            public RelayCommand<AlleleViewModel> RemoveAllele
            {
                get
                {
                    return removeAllele ?? (removeAllele = new RelayCommand<AlleleViewModel>(a =>
                    {
                        Alleles.Remove(a);
                        gene.Alleles.Remove(a.GetAllele());
                        context.Alleles.Remove(a.GetAllele());
                    }));
                }
            }

            public Gene GetGene()
            {
                return gene;
            }
        }

        public class ChromosomeViewModel : ViewModelBase
        {
            private GenesisContext context;
            private Chromosome chromosome;

            public ChromosomeViewModel(Chromosome chromosome, GenesisContext context)
            {
                this.chromosome = chromosome;
                this.context = context;
                Genes = new ObservableCollection<GeneViewModel>(chromosome.Genes.Select(g => new GeneViewModel(g, context)));
            }

            public ObservableCollection<GeneViewModel> Genes { get; protected set; }

            public string Name
            {
                get
                {
                    return chromosome.Name;
                }
                set
                {
                    RaisePropertyChanging(() => Name);
                    chromosome.Name = value;
                    RaisePropertyChanged(() => Name);
                }
            }

            private RelayCommand addTrait;
            public RelayCommand AddTrait
            {
                get
                {
                    return addTrait ?? (addTrait = new RelayCommand(() =>
                    {
                        var gene = new Gene()
                        {
                            Name = "NEW GENE"
                        };
                        
                        chromosome.Genes.Add(gene);
                        this.Genes.Add(new GeneViewModel(gene, context));
                    }));
                }
            }

            private RelayCommand<GeneViewModel> removeTrait;
            public RelayCommand<GeneViewModel> RemoveTrait
            {
                get
                {
                    return removeTrait ?? (removeTrait = new RelayCommand<GeneViewModel>((g) =>
                    {
                        chromosome.Genes.Remove(g.GetGene());
                        this.Genes.Remove(g);
                        context.Genes.Remove(g.GetGene());
                    }));
                }
            }
        }

        #endregion

        private GenesisContext context;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public SettingsViewModel()
        {
            MessengerInstance.Register<GenericMessage<Message>>(this, m =>
            {
                if (m.Target != this)
                    return;

                switch (m.Content)
                {
                    case Message.Refresh:
                        Refresh();
                        break;
                }
            });            
        }

        private void Refresh()
        {
            if (context != null)
                context.Dispose();
            context = new GenesisContext();

            context.Species.Load();

            RaisePropertyChanged(() => Chromosomes);
            RaisePropertyChanged(() => Species);
        }

        public ObservableCollection<ChromosomeViewModel> Chromosomes
        {
            get
            {
                if (context == null)
                    return null;

                return new ObservableCollection<ChromosomeViewModel>(context.Chromosomes.ToList().Select(ch => new ChromosomeViewModel(ch, context)));
            }
        }

        public ObservableCollection<Species> Species
        {
            get
            {
                return context.Species.Local;
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
                        try
                        {
                            context.SaveChanges();
                        }
                        catch (DbEntityValidationException e)
                        {
                            foreach (var err in e.EntityValidationErrors)
                            {
                                foreach (var msg in err.ValidationErrors)
                                {
                                    Console.WriteLine("{1} ({0}): {2} - {3}", err.Entry.Entity.GetType(), err.Entry.Entity, msg.PropertyName, msg.ErrorMessage);
                                }
                            }
                            throw;
                        }
                    });
                }

                return save;
            }
        }
    }
}