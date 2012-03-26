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

    public class FstViewModel : ViewModelBase
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
        public FstViewModel()
        {
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
                    context.Species.ToList();
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


                        RaisePropertyChanged(() => FstAnalysis);
                        RaisePropertyChanged(() => Species);
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
                Set(() => SelectedFstAnalysis, ref selectedFstAnalysis, value);
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
                        if (selectedFstAnalysis != null)
                        {
                            FstAnalysis.Remove(selectedFstAnalysis);
                            SelectedFstAnalysis = null;
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
                    analyze = new RelayCommand(async () =>
                    {
                        analyzing = true;

                        var selectedGenes = Genes.Where(g => g.Selected).Select(g => g.Gene).ToList();

                        var analysis = await Task.Run(() =>
                        {
                            var frequencies = context.Localities.ToList().Select(l =>
                            {
                                var alleles = (from mouse in l.Mice
                                               from record in mouse.Alleles
                                               where selectedGenes.Contains(record.Allele.Gene)
                                               select new { Mouse = mouse, Allele = record.Allele.Species }).ToList();

                                double frequency = 0;
                                int sampleSize = 0;
                                if (alleles.Count > 0)
                                {
                                    double spec = alleles.Where(s => s.Allele == SelectedSpecies).Count();
                                    frequency = spec / alleles.Count;

                                    sampleSize = alleles.Select(a => a.Mouse).Distinct().Count();
                                }

                                return new Frequency
                                {
                                    Locality = l,
                                    SampleSize = sampleSize,
                                    Value = frequency
                                };
                            })
                            .Where(r => r.SampleSize > 0)
                            .ToList();

                            var n = frequencies.Sum(f => f.SampleSize);
                            var Hs = frequencies.Sum(f => f.SampleSize * 2 * f.Value * (1 - f.Value)) / n;
                            var pt = frequencies.Sum(f => f.SampleSize * f.Value) / n;
                            var Ht = 2 * pt * (1 - pt);

                            var result = new FstAnalysis();
                            result.Analyzed = DateTime.Now;
                            result.Name = AnalysisName;
                            result.Fst = (Ht - Hs) / Ht;
                            return result;
                        });

                        context.FstAnalysis.Add(analysis);
                        context.SaveChanges();
                        analyzing = false;
                        Refresh.Execute(null);
                        
                    });
                }

                return analyze;
            }
        }
    }
}