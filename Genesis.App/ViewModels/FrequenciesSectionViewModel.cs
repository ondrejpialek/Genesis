using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Genesis.Analysis;
using Genesis.Extensions;
using Microsoft.Win32;

namespace Genesis.ViewModels
{

    public class FrequenciesSectionViewModel : SectionViewModel
    {      
        public class NominalTraitViewModel : PropertyChangedBase {
            public NominalTraitViewModel(NominalTrait trait) {
                Trait = trait;
            }

            public NominalTrait Trait { get; }

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
        public FrequenciesSectionViewModel()
        {
            DisplayName = "Frequencies";
            Order = 20;

            Traits = new ObservableCollection<NominalTraitViewModel>();
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

        public ObservableCollection<NominalTraitViewModel> Traits
        {
            get;
            protected set;
        }


        protected override void OnActivate()
        {
            base.OnActivate();
            ReloadData(); 
        }


        private FrequencyAnalysis selectedFrequencyAnalysis;
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

        public void DeleteAnalysis(FrequencyAnalysis analysis)
        {
            FrequencyAnalysis.Remove(analysis);
            context.SaveChanges();
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
            currentAnalysis = new FrequencyAnalyzer(AnalysisName, new FrequencyAnalyzer.Settings()
            {
                Traits = Traits.Where(g => g.Selected).Select(g => g.Trait).ToList()
            });

            var context = new GenesisContext();
            var result = await Task.Run(() => currentAnalysis.Analyse(context));
            context.FrequencyAnalysis.Add(result);
            context.SaveChanges();
            ReloadData();
        }

        public bool CanAnalyze()
        {
            return currentAnalysis == null || currentAnalysis.Done;
        }

        private void ReloadData()
        {
            if (context != null)
                context.Dispose();

            context = new GenesisContext();

            Traits.Clear();
            foreach (var gene in context.Genes.OrderBy(g => g.StartBasePair))
            {
                Traits.Add(new NominalTraitViewModel(gene));
            }

            NotifyOfPropertyChange(() => FrequencyAnalysis);
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

        public async void Export()
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
        }

        public bool CanExecute()
        {
            return !exporting && SelectedAnalysis.Count > 0;
        }
    }
}