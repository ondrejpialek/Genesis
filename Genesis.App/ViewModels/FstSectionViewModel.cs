using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Genesis.Analysis;

namespace Genesis.ViewModels
{

    public class FstSectionViewModel : SectionViewModel
    {      
        public class GeneViewModel : ViewModelBase {
            public GeneViewModel(Gene gene) {
                this.gene = gene;
            }

            private Gene gene;
            public Gene Gene
            {
                get
                {
                    return gene;
                }
            }

            private bool selected = false;
            public bool Selected
            {
                get
                {
                    return selected;
                }
                set
                {
                    this.Set(() => Selected, ref selected, value);
                }
            }
        }

        private GenesisContext context;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public FstSectionViewModel()
        {
            DisplayName = "Fst";
            Order = 30;
            Genes = new ObservableCollection<GeneViewModel>();
        }

        public ObservableCollection<FstAnalysis> FstAnalysis
        {
            get
            {
                if (context != null)
                {
                    context.FstAnalysis.ToList();
                    return context.FstAnalysis.Local;
                }
                return null;
            }
        }

        public ObservableCollection<GeneViewModel> Genes
        {
            get;
            protected set;
        }

        public ObservableCollection<Species> Species
        {
            get
            {
                if (context != null)
                {
                    context.Species.OrderByDescending(s => s.Id).ToList();
                    return context.Species.Local;
                }
                return null;
            }
        }

        private Species selectedSpecies = null;
        public Species SelectedSpecies
        {
            get
            {
                return selectedSpecies;
            }
            set
            {
                this.Set(() => SelectedSpecies, ref selectedSpecies, value);
            }
        }

        private RelayCommand refresh;
        public RelayCommand Refresh
        {
            get
            {
                if (refresh == null)
                {
                    refresh = new RelayCommand(() =>
                    {                       
                        if (context != null)
                            context.Dispose();

                        context = new GenesisContext();

                        Genes.Clear();
                        foreach (var gene in context.Genes)
                        {
                            Genes.Add(new GeneViewModel(gene));
                        }


                        NotifyOfPropertyChange(() => FstAnalysis);
                        NotifyOfPropertyChange(() => Species);
                        SelectedSpecies = Species.FirstOrDefault();
                    });
                }

                return refresh;
            }
        }


        private FstAnalysis selectedFstAnalysis = null;
        public FstAnalysis SelectedFstAnalysis
        {
            get
            {
                return selectedFstAnalysis;
            }
            set
            {
                this.Set(() => SelectedFstAnalysis, ref selectedFstAnalysis, value);
            }
        }

        private RelayCommand<FstAnalysis> deleteAnalysis;
        public RelayCommand<FstAnalysis> DeleteAnalysis
        {
            get
            {
                if (deleteAnalysis == null)
                {
                    deleteAnalysis = new RelayCommand<FstAnalysis>((a) =>
                    {
                        FstAnalysis.Remove(a);
                        context.SaveChanges();
                        if (SelectedFstAnalysis == a)
                            SelectedFstAnalysis = FstAnalysis.FirstOrDefault();
                    });
                }

                return deleteAnalysis;
            }
        }

        
        private string analysisName;
        public string AnalysisName
        {
            get
            {
                return analysisName;
            }
            set
            {
                this.Set(() => AnalysisName, ref analysisName, value);
            }
        }

        private RelayCommand analyze;
        public RelayCommand Analyze
        {
            get
            {
                if (analyze == null)
                {
                    analyze = new RelayCommand(async () =>
                    {
                        currentAnalysis = new FstAnalyzer(AnalysisName, new FstAnalyzer.Settings()
                        {
                            Genes = Genes.Where(g => g.Selected).Select(g => g.Gene).ToList(),
                            Species = SelectedSpecies
                        });

                        var context = new GenesisContext();
                        var result = await Task.Run(() => currentAnalysis.Analyse(context));
                        context.FstAnalysis.Add(result);
                        context.SaveChanges();
                    }, () => currentAnalysis == null || currentAnalysis.Done);
                }

                return analyze;
            }
        }

        private FstAnalyzer currentAnalysis { get; set; }
    }
}