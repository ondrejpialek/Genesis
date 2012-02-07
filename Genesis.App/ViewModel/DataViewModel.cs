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

        FeatureSet localities;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public DataViewModel()
        {
            Data = new ObservableCollection<PushpinViewModel>();
            Mice = new ObservableCollection<object>();
            FrequencyAnalysis = new ObservableCollection<FrequencyAnalysis>();
        }

        public ObservableCollection<FrequencyAnalysis> FrequencyAnalysis
        {
            get;
            protected set;
        }

        public ObservableCollection<object> Mice
        {
            get;
            protected set;
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
            }
        }

        private RelayCommand<string> changeView;

        /// <summary>
        /// Gets the ChangeView.
        /// </summary>
        public RelayCommand<string> ChangeView
        {
            get
            {
                return changeView
                    ?? (changeView = new RelayCommand<string>(
                                          p =>
                                          {
                                              view = (MapView)Enum.Parse(typeof(MapView), p);
                                              Refresh.Execute(null);
                                          }));
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
                        context = new GenesisContext();
                        
                        Mice.Clear();
                        foreach (var mice in context.Mice)
                        {
                            Mice.Add(mice);
                        }

                        FrequencyAnalysis.Clear();
                        foreach (var analysis in context.FrequencyAnalysis)
                        {
                            FrequencyAnalysis.Add(analysis);
                        }

                        Data.Clear();
                        if (localities != null)
                        {
                            localities.Features.Clear();
                        }

                        if (View == MapView.Localities)
                        {
                            foreach (var locality in context.Localities)
                            {
                                if (string.IsNullOrEmpty(locality.Code))
                                    continue;

                                if (locality.Location != null)
                                {
                                    Data.Add(new PushpinViewModel(locality, locality.Code));

                                    if (localities != null)
                                    {
                                        Coordinate coord = new Coordinate(locality.Location.Longitude ?? 0, locality.Location.Latitude ?? 0);
                                        DotSpatial.Topology.Point p = new DotSpatial.Topology.Point(coord);
                                        localities.AddFeature(p);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var frequency in FrequencyAnalysis.First().Frequencies)
                            {
                                if (string.IsNullOrEmpty(frequency.Locality.Code))
                                    continue;

                                if (frequency.Locality.Location != null)
                                {
                                    Data.Add(new PushpinViewModel(frequency.Locality, string.Format("{0} {1:F2} ({2})", frequency.Locality.Code, frequency.Value, frequency.SampleSize)));

                                    if (localities != null)
                                    {
                                        Coordinate coord = new Coordinate(frequency.Locality.Location.Longitude ?? 0, frequency.Locality.Location.Latitude ?? 0);
                                        DotSpatial.Topology.Point p = new DotSpatial.Topology.Point(coord);
                                        localities.AddFeature(p);
                                    }
                                }
                            }
                        }
                    });
                }

                return refresh;
            }
        }

        DotSpatial.Controls.Map spatialMap = null;
        private RelayCommand<DotSpatial.Controls.Map> spatialMapLoaded;

        /// <summary>
        /// Gets the SpatialMapLoaded.
        /// </summary>
        public RelayCommand<DotSpatial.Controls.Map> SpatialMapLoaded
        {
            get
            {
                return spatialMapLoaded
                    ?? (spatialMapLoaded = new RelayCommand<DotSpatial.Controls.Map>(
                                          map =>
                                          {
                                              spatialMap = map;
                                              if (spatialMaplegend != null)
                                              {
                                                  spatialMap.Legend = spatialMaplegend;
                                              }

                                              spatialMap.FunctionMode = DotSpatial.Controls.FunctionMode.Pan;

                                              localities = new FeatureSet(FeatureType.Point);
                                              localities.Projection = KnownCoordinateSystems.Geographic.World.WGS1984;

                                              spatialMap.Layers.Add(localities);
                                              Refresh.Execute(null);
                                          }));
            }
        }

        DotSpatial.Controls.Legend spatialMaplegend = null;
        private RelayCommand<DotSpatial.Controls.Legend> spatialMapLegendLoaded;
        public RelayCommand<DotSpatial.Controls.Legend> SpatialMapLegendLoaded
        {
            get
            {
                return spatialMapLegendLoaded
                    ?? (spatialMapLegendLoaded = new RelayCommand<DotSpatial.Controls.Legend>(
                                          legend =>
                                          {
                                              spatialMaplegend = legend;
                                              if (spatialMap != null)
                                              {
                                                  spatialMap.Legend = spatialMaplegend;
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
    }
}