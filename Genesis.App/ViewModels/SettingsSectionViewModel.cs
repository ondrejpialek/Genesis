using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using Caliburn.Micro;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace Genesis.ViewModels
{

    public class SettingsSectionViewModel : SectionViewModel
    {                   

        public SettingsSectionViewModel()
        {
            DisplayName = "Settings";
            Order = 100;
        }

        #region tree view models

        public class AlleleViewModel : PropertyChangedBase
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
                    a.Value = value;
                    NotifyOfPropertyChange(() => Value);
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
                    a.Species = value;
                    NotifyOfPropertyChange(() => Species);
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

        public class GeneViewModel : PropertyChangedBase
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
                    gene.Name = value;
                    NotifyOfPropertyChange(() => Name);
                }
            }

            public int StartBasePair
            {
                get
                {
                    return gene.StartBasePair;
                }
                set
                {
                    gene.StartBasePair = value;
                    NotifyOfPropertyChange(() => StartBasePair);
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

            public void RemoveAllele(AlleleViewModel allele)
            {
                Alleles.Remove(allele);
                gene.Alleles.Remove(allele.GetAllele());
                context.Alleles.Remove(allele.GetAllele());
            }

            public Gene GetGene()
            {
                return gene;
            }
        }

        public class ChromosomeViewModel : PropertyChangedBase
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
                    chromosome.Name = value;
                    NotifyOfPropertyChange(() => Name);
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

        protected override void OnActivate()
        {
            base.OnActivate();
            
            if (context != null)
                context.Dispose();
            context = new GenesisContext();

            context.Species.Load();

            NotifyOfPropertyChange(() => Chromosomes);
            NotifyOfPropertyChange(() => Species);
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
                if (context == null)
                    return null;

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

        private RelayCommand recreate;
        public RelayCommand Recreate
        {
            get
            {
                return recreate ?? (recreate = new RelayCommand(() =>
                {
                    context.Database.Delete();
                    Refresh();
                }));
            }
        }

        private RelayCommand reset;
        public RelayCommand Reset
        {
            get
            {
                return reset ?? (reset = new RelayCommand(() =>
                {
                    var localities = context.Localities.ToList();
                    foreach (var l in localities)
                        context.Localities.Remove(l);
                    context.SaveChanges();
                    Refresh();
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