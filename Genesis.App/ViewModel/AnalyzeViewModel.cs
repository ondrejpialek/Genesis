using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Linq;
using System;
using System.Data.Entity;
using Genesis.Analysis;

namespace Genesis.ViewModel
{

    public class AnalyzeViewModel : ViewModelBase
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
                    Set(() => Selected, ref selected, value);
                }
            }
        }

        private GenesisContext context;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public AnalyzeViewModel()
        {
            Genes = new ObservableCollection<GeneViewModel>();
        }

        public ObservableCollection<FrequencyAnalysis> FrequencyAnalysis
        {
            get
            {
                if (context != null)
                {
                    context.FrequencyAnalysis.Load();
                    return context.FrequencyAnalysis.Local;
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

                    context.Species.Load();
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
                Set(() => SelectedSpecies, ref selectedSpecies, value);
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

                        RaisePropertyChanged(() => FrequencyAnalysis);
                        RaisePropertyChanged(() => Species);
                        SelectedSpecies = Species.FirstOrDefault();
                    });
                }

                return refresh;
            }
        }


        private FrequencyAnalysis selectedFrequencyAnalysis = null;
        public FrequencyAnalysis SelectedFrequencyAnalysis
        {
            get
            {
                return selectedFrequencyAnalysis;
            }
            set
            {
                Set(() => SelectedFrequencyAnalysis, ref selectedFrequencyAnalysis, value);
            }
        }

        private RelayCommand<FrequencyAnalysis> deleteAnalysis;
        public RelayCommand<FrequencyAnalysis> DeleteAnalysis
        {
            get
            {
                if (deleteAnalysis == null)
                {
                    deleteAnalysis = new RelayCommand<FrequencyAnalysis>((a) =>
                    {
                        FrequencyAnalysis.Remove(a);
                        context.SaveChanges();
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
                Set(() => AnalysisName, ref analysisName, value);
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
                        currentAnalysis = new FrequencyAnalyzer(AnalysisName, new FrequencyAnalyzer.Settings()
                        {
                            Genes = Genes.Where(g => g.Selected).Select(g => g.Gene).ToList(),
                            Species = SelectedSpecies
                        });

                        var context = new GenesisContext();
                        var result = await Task.Run(() => currentAnalysis.Analyse(context));
                        context.FrequencyAnalysis.Add(result);
                        context.SaveChanges();
                        Refresh.Execute(null);
                    }, () => currentAnalysis == null || currentAnalysis.Done);
                }

                return analyze;
            }
        }

        private FrequencyAnalyzer currentAnalysis { get; set; }
    }
}