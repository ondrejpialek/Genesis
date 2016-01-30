using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Genesis.Analysis;

namespace Genesis.ViewModels
{

    public class FstSectionViewModel : SectionViewModel
    {      
        public class GeneViewModel : PropertyChangedBase {
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

        public new void Refresh()
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

        public void DeleteAnalysis(FstAnalysis fstAnalysis)
        {
            FstAnalysis.Remove(fstAnalysis);
            context.SaveChanges();
            if (SelectedFstAnalysis == fstAnalysis)
                SelectedFstAnalysis = FstAnalysis.FirstOrDefault();
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

        public async void Analyze()
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
        }

        public bool CanAnalyze()
        {
            return currentAnalysis == null || currentAnalysis.Done;
        }

        private FstAnalyzer currentAnalysis { get; set; }
    }
}