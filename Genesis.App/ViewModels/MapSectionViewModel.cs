using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Input;
using Caliburn.Micro;
using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Data.Forms;
using DotSpatial.Projections;
using DotSpatial.Topology;
using Microsoft.Win32;
using Location = Microsoft.Maps.MapControl.WPF.Location;
using Point = DotSpatial.Topology.Point;

namespace Genesis.ViewModels
{
    public class MapSectionViewModel : SectionViewModel
    {
        public enum MapView
        {
            Localities,
            Frequencies
        };

        public enum Unit
        {
            cm,
            px
        };

        private const double CM_PER_INCH = 2.54;
        private readonly ImageFormat[] formats = new[] {ImageFormat.Bmp, ImageFormat.Jpeg, ImageFormat.Png, ImageFormat.Gif, ImageFormat.Tiff};
        private readonly Unit[] units = new[] {Unit.cm, Unit.px};

        private GenesisContext context;
        private double height = 600;
        private ImageFormat imageFormat = ImageFormat.Png;
        private int resolution = 300;

        private FrequencyAnalysis selectedAnalysis;
        private Unit selectedUnit = Unit.px;
        private IMapFeatureLayer spatialDataLayer;
        private Map spatialMap;

        private Legend spatialMaplegend;
        private MapView view = MapView.Localities;
        private double width = 800;

        /// <summary>
        ///   Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MapSectionViewModel()
        {
            DisplayName = "Map";
            Order = 10;

            Data = new ObservableCollection<PushpinViewModel>();
            Points = new ObservableCollection<Point>();
        }

        public FrequencyAnalysis SelectedAnalysis
        {
            get
            {
                return selectedAnalysis;
            }
            set
            {
                this.Set(() => SelectedAnalysis, ref selectedAnalysis, value);
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

        public ObservableCollection<PushpinViewModel> Data { get; protected set; }

        public ObservableCollection<Point> Points;
        private MapLineLayer layerBeingEdited;

        public MapView View
        {
            get
            {
                return view;
            }
            set
            {
                this.Set(() => View, ref view, value);
                UpdateData();
            }
        }

        public void ChangeView(string view)
        {
            View = (MapView)Enum.Parse(typeof(MapView), view);
        }

        public void SpatialMapLoaded(Map map)
        {
            InitGisMap(map);
        }

        public void SpatialMapLegendLoaded(Legend legend)
        {
            if (spatialMaplegend == null)
            {
                spatialMaplegend = legend;
                if (spatialMap != null)
                {
                    spatialMap.Legend = spatialMaplegend;
                }
            }
        }

        public void AddLayer()
        {
            List<IDataSet> files = DataManager.DefaultDataManager.OpenFiles();

            if (files == null)
                return;

            foreach (IDataSet file in files)
            {
                spatialMap.Layers.Insert(spatialMap.Layers.Count - 1, file);
            }
        }

        public void ClearLayers()
        {
            spatialMap.ClearLayers();
            if (spatialDataLayer != null)
            {
                spatialDataLayer.Dispose();
                spatialDataLayer = null;
            }
        }

        public void AddPolyline()
        { 
            var layer = new MapLineLayer
            {
                Name = "Polyline",
                LegendText = "Polyline"
            };

            spatialMaplegend.MouseDown += (sender, args) =>
            {
                if (layer.IsSelected)
                {
                    layerBeingEdited = layer;
                    
                    Points.Clear();

                    for (int i = 0; i < layer.DataSet.DataTable.Rows.Count; i++)
                    {
                        var point = layer.DataSet.GetFeature(i).BasicGeometry as Point;

                        Points.Add(point);
                    }
                }
            };


            var dataset =  new FeatureSet(FeatureType.Point);

            dataset.Projection = KnownCoordinateSystems.Geographic.World.WGS1984;

            dataset.DataTable.BeginInit();
            //dataset.DataTable.Columns.Add(new DataColumn("Code", typeof (string)));
                layer.DataSet = dataset;

            dataset.DataTable.EndLoadData();
            dataset.InvalidateVertices();

            spatialMap.Layers.Add(layer);

        }

        public void AddPoint()
        {
            var point = new Point(spatialMap.ViewExtents.Center);
            IFeature feature = layerBeingEdited.DataSet.AddFeature(point);
            Points.Add(point);
        }

        public bool CanAddPoint()
        {
            return layerBeingEdited != null;
        }

        public void SaveImage()
        {
            var dlg = new SaveFileDialog
            {
                FileName = "map",
                DefaultExt = ".png",
                Filter = "PNG files|*.png|JPEG files|*.jpg;*.jpeg|BMP files|*.bmp|GIF files|*.gif|TIFF file|*.tiff"
            };

            if (dlg.ShowDialog() == true)
            {
                int w = 0, h = 0;
                switch (SelectedUnit)
                {
                    case Unit.cm:
                        w = (int) (Width/CM_PER_INCH*resolution);
                        h = (int) (Height/CM_PER_INCH*resolution);
                        break;
                    case Unit.px:
                        w = (int) Width;
                        h = (int) Height;
                        break;
                }

                var image = new Bitmap(w, h);
                image.SetResolution(resolution, resolution);

                Graphics graphics = Graphics.FromImage(image);
                spatialMap.Print(graphics, new Rectangle(0, 0, w, h));
                graphics.Flush();

                image.Save(dlg.FileName, ImageFormat);
            }
        }

        public bool CanSaveImage()
        {
            return spatialMap != null;
        }

        public Unit[] Units
        {
            get
            {
                return units;
            }
        }

        public Unit SelectedUnit
        {
            get
            {
                return selectedUnit;
            }
            set
            {
                this.Set(() => SelectedUnit, ref selectedUnit, value);
            }
        }

        public ImageFormat[] Formats
        {
            get
            {
                return formats;
            }
        }

        public ImageFormat ImageFormat
        {
            get
            {
                return imageFormat;
            }
            set
            {
                this.Set(() => ImageFormat, ref imageFormat, value);
            }
        }

        public int Resolution
        {
            get
            {
                return resolution;
            }
            set
            {
                this.Set(() => Resolution, ref resolution, value);
            }
        }

        public double Height
        {
            get
            {
                return height;
            }
            set
            {
                value = SelectedUnit == Unit.px ? Math.Round(value) : Math.Round(value, 2);
                this.Set(() => Height, ref height, value);
            }
        }

        public double Width
        {
            get
            {
                return width;
            }
            set
            {
                value = SelectedUnit == Unit.px ? Math.Round(value) : Math.Round(value, 2);
                this.Set(() => Width, ref width, value);
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            if (context != null)
                context.Dispose();
            context = new GenesisContext();

            NotifyOfPropertyChange(() => FrequencyAnalysis);
            NotifyOfPropertyChange(() => Localities);

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
                foreach (Locality locality in context.Localities)
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
                    foreach (Frequency frequency in selectedAnalysis.Frequencies)
                    {
                        if (string.IsNullOrEmpty(frequency.Locality.Code) || frequency.Locality.Location == null)
                            continue;

                        Data.Add(new PushpinViewModel(frequency.Locality,
                                                      string.Format("{0} {1:F2} ({2})", frequency.Locality.Code, frequency.Value, frequency.SampleSize)));
                    }
                }
            }
        }

        private void InitGisMap(Map map)
        {
            if (spatialMap == null)
            {
                spatialMap = map;

                spatialMap.Resize += spatialMap_Resize;

                if (spatialMaplegend != null)
                {
                    spatialMap.Legend = spatialMaplegend;
                }

                spatialMap.FunctionMode = FunctionMode.Pan;
                UpdateGisData();
            }
        }

        private void spatialMap_Resize(object sender, EventArgs e)
        {
            Width = SelectedUnit == Unit.px ? spatialMap.Width : spatialMap.Width/(double) resolution*CM_PER_INCH;
            Height = SelectedUnit == Unit.px ? spatialMap.Height : spatialMap.Height/(double) resolution*CM_PER_INCH;
        }

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
                    dataset.DataTable.Columns.Add(new DataColumn("Code", typeof (string)));
                    spatialDataLayer = spatialMap.Layers.Add(dataset);
                }
                else
                {
                    dataset = spatialDataLayer.DataSet;
                    spatialDataLayer.DataSet.DataTable.BeginInit();
                    dataset.Features.Clear();
                    dataset.DataTable.Clear();
                }


                if (View == MapView.Localities)
                {
                    if (dataset.DataTable.Columns.Count == 2)
                        dataset.DataTable.Columns.RemoveAt(1);

                    foreach (Locality locality in context.Localities)
                    {
                        if (string.IsNullOrEmpty(locality.Code) || locality.Location == null)
                            continue;

                        var coord = new Coordinate(locality.Location.Longitude ?? 0, locality.Location.Latitude ?? 0);
                        IFeature feature = dataset.AddFeature(new Point(coord));

                        feature.DataRow["Code"] = locality.Code;
                    }
                }
                else
                {
                    if (dataset.DataTable.Columns.Count == 1)
                        dataset.DataTable.Columns.Add(new DataColumn("Frequency", typeof (double)));

                    if (selectedAnalysis != null)
                    {
                        foreach (Frequency frequency in selectedAnalysis.Frequencies)
                        {
                            if (string.IsNullOrEmpty(frequency.Locality.Code) || frequency.Locality.Location == null)
                                continue;

                            var coord = new Coordinate(frequency.Locality.Location.Longitude ?? 0, frequency.Locality.Location.Latitude ?? 0);
                            IFeature feature = dataset.AddFeature(new Point(coord));
                            feature.DataRow["Frequency"] = frequency.Value;
                            feature.DataRow["Code"] = frequency.Locality.Code;
                        }
                    }
                }

                dataset.DataTable.EndLoadData();
                dataset.InvalidateVertices();
            }
        }

        public class PushpinViewModel : PropertyChangedBase
        {
            private readonly Locality locality;
            private string label;

            public PushpinViewModel(Locality locality, string label)
            {
                this.locality = locality;
                this.label = label;
            }

            public Location Location
            {
                get
                {
                    if (locality.Location == null)
                        return null;

                    return new Location(locality.Location.Latitude.Value, locality.Location.Longitude.Value);
                }
            }

            public string Label
            {
                get
                {
                    return label;
                }
                set
                {
                    this.Set(() => Label, ref label, value);
                }
            }
        }
    }
}