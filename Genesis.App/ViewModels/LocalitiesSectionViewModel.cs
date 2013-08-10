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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using Location = Microsoft.Maps.MapControl.WPF.Location;
using Point = DotSpatial.Topology.Point;

namespace Genesis.ViewModels
{
    public class LocalitiesSectionViewModel : Screen, ISectionViewModel
    {
        private GenesisContext context;

        /// <summary>
        ///   Initializes a new instance of the MainViewModel class.
        /// </summary>
        public LocalitiesSectionViewModel()
        {
            DisplayName = "Localities";

            Localities = new BindableCollection<Locality>();
        }

        public BindableCollection<Locality> Localities { get; protected set; }

        protected override void OnActivate()
        {
            base.OnActivate();

            if (context != null)
                context.Dispose();

            context = new GenesisContext();

            Localities.Clear();
            Localities.AddRange(context.Localities);
        }
    }
}