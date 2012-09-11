using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Data.Forms;
using DotSpatial.Projections;
using DotSpatial.Topology;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Genesis;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

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

            Messenger.Default.Register<GenericMessage<Message>>(this, m =>
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
            if (context != null)
                context.Dispose();
            context = new GenesisContext();

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

        private RelayCommand<Map> spatialMapLoaded;
        public RelayCommand<Map> SpatialMapLoaded
        {
            get
            {
                return spatialMapLoaded ?? (spatialMapLoaded = new RelayCommand<Map>(map => InitGisMap(map)));
            }
        }

        Map spatialMap = null;
        private void InitGisMap(Map map)
        {
            if (spatialMap == null)
            {
                spatialMap = map;
                if (spatialMaplegend != null)
                {
                    spatialMap.Legend = spatialMaplegend;
                }

                spatialMap.FunctionMode = FunctionMode.Pan;

                UpdateGisData();
            }
        }

        IMapFeatureLayer spatialDataLayer = null;
        private void UpdateGisData()
        {
            if (spatialMap != null)
            {
                IFeatureSet dataset;
                if (spatialDataLayer == null)
                {
                    dataset = new FeatureSet(FeatureType.Point);
                    
                    dataset.Projection = KnownCoordinateSystems.Geographic.World.WGS1984;

                    dataset.DataTable.BeginInit();
                    dataset.DataTable.Columns.Add(new DataColumn("Code", typeof(string)));
                    spatialDataLayer = spatialMap.Layers.Add(dataset);
                } else {
                    dataset = spatialDataLayer.DataSet;
                    spatialDataLayer.DataSet.DataTable.BeginInit();
                    dataset.Features.Clear();
                    dataset.DataTable.Clear();
                }             

                

                if (View == MapView.Localities)
                {
                    if (dataset.DataTable.Columns.Count == 2)
                        dataset.DataTable.Columns.RemoveAt(1);

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
                    if (dataset.DataTable.Columns.Count == 1)
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

                dataset.DataTable.EndLoadData();
                dataset.InvalidateVertices();
            }
        }

        Legend spatialMaplegend = null;
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
                return addLayer ?? (addLayer = new RelayCommand(() => {
                    var files = DataManager.DefaultDataManager.OpenFiles();

                    foreach (var file in files)
                    {
                        spatialMap.Layers.Insert(spatialMap.Layers.Count - 1, file);
                    }
                }));
            }
        }

        private RelayCommand clearLayers;
        public RelayCommand ClearLayers
        {
            get
            {
                return clearLayers ?? (clearLayers = new RelayCommand(() => {
                    spatialMap.ClearLayers();
                    if (spatialDataLayer != null)
                    {
                        spatialDataLayer.Dispose();
                        spatialDataLayer = null;
                    }
                }));
            }
        }

        private RelayCommand saveImage;
        public RelayCommand SaveImage
        {
            get
            {
                return saveImage ?? (saveImage = new RelayCommand(() =>
                  {
                      SaveFileDialog dlg = new SaveFileDialog();
                      dlg.FileName = "map";
                      dlg.DefaultExt = ".png";
                      dlg.Filter = "PNG files (.png)|*.png";

                      if (dlg.ShowDialog() == true)
                      {
                          var image = new Bitmap(3000, 3000);

                          var graphics = Graphics.FromImage(image);
                          spatialMap.Print(graphics, new Rectangle(0, 0, 3000, 3000));
                          graphics.Flush();
                          image.Save(dlg.FileName, ImageFormat.Png);
                      }

                  }, () => spatialMap != null));
            }
        }

        public FeatureSet dataset { get; set; }
    }
}