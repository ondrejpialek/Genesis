using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Linq;
using System;

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
        private bool analyzing;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public AnalyzeViewModel()
        {
            FrequencyAnalysis = new ObservableCollection<FrequencyAnalysis>();
            Genes = new ObservableCollection<GeneViewModel>();
            Species = new ObservableCollection<Species>();
        }

        public ObservableCollection<FrequencyAnalysis> FrequencyAnalysis
        {
            get;
            protected set;
        }

        public ObservableCollection<GeneViewModel> Genes
        {
            get;
            protected set;
        }

        public ObservableCollection<Species> Species
        {
            get;
            protected set;
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
                        if (analyzing)
                            return;
                        
                        if (context != null)
                            context.Dispose();

                        context = new GenesisContext();

                        Genes.Clear();
                        foreach (var gene in context.Genes)
                        {
                            Genes.Add(new GeneViewModel(gene));
                        }

                        FrequencyAnalysis.Clear();
                        foreach (var analysis in context.FrequencyAnalysis)
                        {
                            FrequencyAnalysis.Add(analysis);
                        }

                        Species.Clear();
                        foreach (var species in context.Species)
                        {
                            Species.Add(species);

                            if (SelectedSpecies == null)
                            {
                                SelectedSpecies = species;
                            }
                        }
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

        private RelayCommand deleteAnalysis;
        public RelayCommand DeleteAnalysis
        {
            get
            {
                if (deleteAnalysis == null)
                {
                    deleteAnalysis = new RelayCommand(() =>
                    {
                        if (selectedFrequencyAnalysis != null)
                        {
                            context.FrequencyAnalysis.Remove(selectedFrequencyAnalysis);
                            FrequencyAnalysis.Remove(selectedFrequencyAnalysis);
                            SelectedFrequencyAnalysis = null;
                        }
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
                    analyze = new RelayCommand(() =>
                    {
                        analyzing = true;

                        var selectedGenes = Genes.Where(g => g.Selected).Select(g => g.Gene).ToList();

                        var analysis = new FrequencyAnalysis();
                        analysis.Analyzed = DateTime.Now;
                        analysis.Name = AnalysisName;

                        context.Localities.ToObservable(new NewThreadScheduler()).Select(l =>
                        {
                            var alleles = (from mouse in l.Mice
                                           from record in mouse.Alleles
                                           where selectedGenes.Contains(record.Allele.Gene)
                                           select new { Mouse = mouse, Allele = record.Allele.Species}).ToList();

                            double frequency = 0;
                            int sampleSize = 0;
                            if (alleles.Count > 0)
                            {
                                double spec = alleles.Where(s => s.Allele == SelectedSpecies).Count();
                                frequency = spec / alleles.Count;

                                sampleSize = alleles.Select(a => a.Mouse).Distinct().Count();
                            }

                            return new Frequency {
                                Locality = l,
                                SampleSize = sampleSize,
                                Value = frequency };
                        })
                        .Where(r => r.SampleSize > 0)  
                        .ObserveOn(DispatcherScheduler.Instance)                    
                        .Subscribe(f =>
                        {
                            analysis.Frequencies.Add(f);
                        }, () =>
                        {
                            context.FrequencyAnalysis.Add(analysis);
                            context.SaveChanges();
                            analyzing = false;
                            Refresh.Execute(null);
                        });
                    });
                }

                return analyze;
            }
        }
    }
}