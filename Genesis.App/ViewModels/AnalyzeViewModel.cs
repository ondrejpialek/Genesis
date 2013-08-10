using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Genesis.Analysis;
using Genesis.Extensions;
using Microsoft.Win32;

namespace Genesis.ViewModels
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
                    this.Set(() => Selected, ref selected, value);
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
            SelectedAnalysis = new ObservableCollection<FrequencyAnalysis>();

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

                    context.Species.OrderByDescending(s => s.Id).Load();
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
                        foreach (var gene in context.Genes.OrderBy(g => g.StartBasePair))
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
                this.Set(() => SelectedFrequencyAnalysis, ref selectedFrequencyAnalysis, value);
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

        public ObservableCollection<FrequencyAnalysis> SelectedAnalysis { get; protected set; }

        private float progress = 0;
        public float Progress
        {
            get
            {
                return progress;
            }
            set
            {
                this.Set(() => Progress, ref progress, value);
            }
        }

        public bool exporting;

        private RelayCommand export;
        public RelayCommand Export
        {
            get
            {
                return export ?? (export = new RelayCommand(async () =>
                {
                    exporting = true;
                    try
                    {
                        var dialog = new SaveFileDialog();
                        if (dialog.ShowDialog() == true)
                        {
                            StreamWriter sw = new StreamWriter(dialog.FileName, false);

                            var selectedAnalysis = SelectedAnalysis.ToList();

                            var cols = new string[] { "Code", "Latitude", "Longitude" }.Union(selectedAnalysis.Select(a => "\"" + a.Name + "\"")).ToArray();
                            await sw.WriteCSVLineAsync(cols);

                            var localities = selectedAnalysis.SelectMany(a => a.Frequencies).Select(f => f.Locality).Distinct().OrderBy(l => l.Code).ToList();
                            var step = 1.0f / localities.Count;
                            Progress = 0;

                            var enumerators = selectedAnalysis.Select(a => a.Frequencies.OrderBy(f => f.Locality.Code).GetEnumerator()).ToArray();
                            var hasElements = enumerators.Select(e => e.MoveNext()).ToArray();

                            foreach (var l in localities)
                            {
                                Progress += step;
                                StringBuilder sb = new StringBuilder();
                                sb.Append(string.Join(",", l.Code, l.Location == null ? string.Empty : l.Location.Latitude.ToString(), l.Location == null ? string.Empty : l.Location.Longitude.ToString()));
                                sb.Append(",");
                                for (var i = 0; i < enumerators.Length; i++)
                                {
                                    if (hasElements[i] && enumerators[i].Current.Locality == l)
                                    {
                                        sb.Append(enumerators[i].Current.Value.ToString());
                                        hasElements[i] = enumerators[i].MoveNext();
                                    }
                                    if (i < enumerators.Length - 1)
                                        sb.Append(",");
                                }
                                await sw.WriteLineAsync(sb.ToString());
                                await Task.Delay(1);
                            }

                            sw.Flush();
                            sw.Close();

                            Progress = 1;
                        }
                    }
                    finally
                    {
                        exporting = false;
                    }
                }, () => !exporting && SelectedAnalysis.Count > 0));
            }
        }


    }
}