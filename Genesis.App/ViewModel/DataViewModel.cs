using System;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Maps.MapControl.WPF;
using System.Linq;
using DotSpatial.Symbology;
using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Topology;
using DotSpatial.Projections;
using System.Data;
using GalaSoft.MvvmLight.Messaging;
using Genesis.Views;
using System.Data.Entity;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;

namespace Genesis.ViewModel
{

    public class DataViewModel : ViewModelBase
    {
        public enum MapView { Localities, Frequencies };
        
        public class PushpinViewModel : ViewModelBase
        {
            private Locality locality;

            public PushpinViewModel(Locality locality, string label)
            {
                this.locality = locality;
                this.label = label;
            }

            public Microsoft.Maps.MapControl.WPF.Location Location
            {
                get
                {
                    if (locality.Location == null)
                        return null;

                    return new Microsoft.Maps.MapControl.WPF.Location(locality.Location.Latitude.Value, locality.Location.Longitude.Value);
                }
            }
            
            private string label;
            public string Label
            {
                get
                {
                    return label;
                }
                set
                {
                    Set(() => Label, ref label, value);
                }
            }
        }

        private GenesisContext context;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public DataViewModel()
        {
            Data = new ObservableCollection<PushpinViewModel>();

            Messenger.Default.Register<Message>(this, m =>
            {
                switch (m)
                {
                    case Message.Refresh:
                        Refresh();
                        break;
                }
            });
        }

        private FrequencyAnalysis selectedAnalysis = null;
        public FrequencyAnalysis SelectedAnalysis
        {
            get
            {
                return selectedAnalysis;
            }
            set
            {
                Set(() => SelectedAnalysis, ref selectedAnalysis, value);
                UpdateData();
            }
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

        public ObservableCollection<Locality> Localities
        {
            get
            {
                if (context != null)
                {
                    context.Localities.Load();
                    return context.Localities.Local;
                }
                return null;
            }
        }

        public ObservableCollection<Mouse> Mice
        {
            get
            {
                if (context != null)
                {
                    context.Mice.Load();
                    return context.Mice.Local;
                }
                return null;
            }
        }

        public ObservableCollection<Gene> Genes
        {
            get
            {
                if (context != null)
                {
                    context.Genes.Load();
                    return context.Genes.Local;
                }
                return null;
            }
        }

        public ObservableCollection<PushpinViewModel> Data
        {
            get;
            protected set;
        }


        private MapView view = MapView.Localities;
        public MapView View
        {
            get
            {
                return view;
            }
            set
            {
                Set(() => View, ref view, value);
                UpdateData();
            }
        }

        private RelayCommand<string> changeView;
        public RelayCommand<string> ChangeView
        {
            get
            {
                return changeView ?? (changeView = new RelayCommand<string>(
                p =>
                {
                    View = (MapView)Enum.Parse(typeof(MapView), p);
                }));
            }
        }

        private void Refresh() {
            context = new GenesisContext();

            RaisePropertyChanged(() => Mice);
            RaisePropertyChanged(() => FrequencyAnalysis);
            RaisePropertyChanged(() => Localities);

            UpdateData();
        }

        private void UpdateData()
        {
            UpdateBingData();
            UpdateGisData();
        }

        private void UpdateBingData()
        {
            Data.Clear();
            if (View == MapView.Localities)
            {
                foreach (var locality in context.Localities)
                {
                    if (string.IsNullOrEmpty(locality.Code) || locality.Location == null)
                        continue;

                    Data.Add(new PushpinViewModel(locality, locality.Code));
                }
            }
            else
            {
                if (selectedAnalysis != null)
                {
                    foreach (var frequency in selectedAnalysis.Frequencies)
                    {
                        if (string.IsNullOrEmpty(frequency.Locality.Code) || frequency.Locality.Location == null)
                            continue;

                        Data.Add(new PushpinViewModel(frequency.Locality,
                            string.Format("{0} {1:F2} ({2})", frequency.Locality.Code, frequency.Value, frequency.SampleSize)));
                    }
                }
            }
        }

        private RelayCommand<DotSpatial.Controls.Map> spatialMapLoaded;
        public RelayCommand<DotSpatial.Controls.Map> SpatialMapLoaded
        {
            get
            {
                return spatialMapLoaded ?? (spatialMapLoaded = new RelayCommand<DotSpatial.Controls.Map>(map => InitGisMap(map)));
            }
        }

        DotSpatial.Controls.Map spatialMap = null;
        private void InitGisMap(DotSpatial.Controls.Map map)
        {
            if (spatialMap == null)
            {
                spatialMap = map;
                if (spatialMaplegend != null)
                {
                    spatialMap.Legend = spatialMaplegend;
                }

                spatialMap.FunctionMode = DotSpatial.Controls.FunctionMode.Pan;

                UpdateGisData();
            }
        }

        IMapFeatureLayer spatialDataLayer = null;
        private void UpdateGisData()
        {
            if (spatialMap != null)
            {
                if (spatialDataLayer != null)
                {
                    spatialMap.Layers.Remove(spatialDataLayer);
                    spatialDataLayer.Dispose();
                }
                /*
                if (spatialDataLayer != null)
                {
                    spatialDataLayer.DataSet.Features.Clear();
                    spatialDataLayer.DataSet.DataTable.Clear();
                }*/

                dataset = new FeatureSet(FeatureType.Point);
                dataset.Projection = KnownCoordinateSystems.Geographic.World.WGS1984;

                dataset.DataTable.Columns.Add(new DataColumn("Code", typeof(string)));

                if (View == MapView.Localities)
                {
                    foreach (var locality in context.Localities)
                    {
                        if (string.IsNullOrEmpty(locality.Code) || locality.Location == null)
                            continue;

                        Coordinate coord = new Coordinate(locality.Location.Longitude ?? 0, locality.Location.Latitude ?? 0);
                        var feature = dataset.AddFeature(new DotSpatial.Topology.Point(coord));

                        feature.DataRow["Code"] = locality.Code;
                    }
                }
                else
                {
                    dataset.DataTable.Columns.Add(new DataColumn("Frequency", typeof(double)));
                    if (selectedAnalysis != null)
                    {
                        foreach (var frequency in selectedAnalysis.Frequencies)
                        {
                            if (string.IsNullOrEmpty(frequency.Locality.Code) || frequency.Locality.Location == null)
                                continue;

                            Coordinate coord = new Coordinate(frequency.Locality.Location.Longitude ?? 0, frequency.Locality.Location.Latitude ?? 0);
                            var feature = dataset.AddFeature(new DotSpatial.Topology.Point(coord));
                            feature.DataRow["Frequency"] = frequency.Value;
                            feature.DataRow["Code"] = frequency.Locality.Code;
                        }
                    }
                }

                spatialDataLayer = spatialMap.Layers.Add(dataset);
            }
        }

        DotSpatial.Controls.Legend spatialMaplegend = null;
        private RelayCommand<Legend> spatialMapLegendLoaded;
        public RelayCommand<Legend> SpatialMapLegendLoaded
        {
            get
            {
                return spatialMapLegendLoaded ?? (spatialMapLegendLoaded = new RelayCommand<Legend>(legend =>
                {
                    if (spatialMaplegend == null)
                    {
                        spatialMaplegend = legend;
                        if (spatialMap != null)
                        {
                            spatialMap.Legend = spatialMaplegend;
                        }
                    }
                }));
            }
        }

        private RelayCommand addLayer;
        public RelayCommand AddLayer
        {
            get
            {
                return addLayer ?? (addLayer = new RelayCommand(() => { spatialMap.AddLayer(); }));
            }
        }

        private RelayCommand clearLayers;
        public RelayCommand ClearLayers
        {
            get
            {
                return clearLayers ?? (clearLayers = new RelayCommand(() => { spatialMap.ClearLayers(); }));
            }
        }

        public FeatureSet dataset { get; set; }

        DataGrid grid;
        private RelayCommand<DataGrid> dataGridLoaded;
        public RelayCommand<DataGrid> DataGridLoaded
        {
            get
            {
                return dataGridLoaded
                    ?? (dataGridLoaded = new RelayCommand<DataGrid>(
                                          g =>
                                          {
                                              grid = g;
                                          }));
            }
        }

        private MouseToAllelesConverter mouseToAllelesConverter;
        private RelayCommand<Gene> addGeneColumn;
        public RelayCommand<Gene> AddColumn
        {
            get
            {
                return addGeneColumn
                    ?? (addGeneColumn = new RelayCommand<Gene>(
                        gene =>
                        {
                            if (grid != null)
                            {
                                var col = new DataGridTextColumn() { Header = gene.Name };
                                grid.Columns.Add(col);
                                var binding = new Binding();
                                binding.Converter = mouseToAllelesConverter ?? (mouseToAllelesConverter = new MouseToAllelesConverter());
                                binding.ConverterParameter = gene;
                                col.Binding = binding;                                                  
                            }
                        }));
            }
        }

        private RelayCommand<Gene> removeGeneColumn;
        public RelayCommand<Gene> RemoveColumn
        {
            get
            {
                return removeGeneColumn
                    ?? (removeGeneColumn = new RelayCommand<Gene>(
                        gene =>
                        {
                            if (grid != null)
                            {
                                var col = grid.Columns.Where(c => c.Header == gene.Name).FirstOrDefault();
                                if (col != null)
                                {
                                    grid.Columns.Remove(col);
                                }
                            }
                        }));
            }
        }
    }
}