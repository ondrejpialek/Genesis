using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesis;
using Microsoft.SqlServer.Types;
using System.Data.Entity.Spatial;

namespace Genesis.Excel
{
    /// <summary>
    /// Sets the longitude component of the <see cref="Locality.Location" /> property.
    /// </summary>
    public class LongitudeLocationComponentCellReader : CellReader<Locality, string>
    {
        public LongitudeLocationComponentCellReader() : base("Longitude") { }

        protected override void Apply(Locality locality, string v)
        {
            double? value = null;
            if (!string.IsNullOrEmpty(v))
            {
                var args = v.Split(' ');
                if (args.Length == 3)
                {
                    try
                    {
                        double sign = 1;
                        double deg = int.Parse(args[0].Substring(0, args[0].Length - 1));
                        if (deg < 0)
                            sign = -1;
                        double min = int.Parse(args[1].Substring(0, args[1].Length - 1)) * sign;
                        double sec = int.Parse(args[2].Substring(0, args[2].Length - 1)) * sign;

                        value = deg + min / 60 + sec / (60 * 60);
                    }
                    catch
                    {

                    }
                }
            }
            if (value.HasValue)
            {
                var latitude = locality.Location == null ? 0 : locality.Location.Latitude ?? 0;
                var point = SqlGeography.Point(latitude, value.Value, 4326);
                var text = new string(point.STAsText().Value);
                var geo = DbSpatialServices.Default.GeographyFromText(text);
                locality.Location = geo;
            }
            else
            {
                locality.Location = null;
            }
        }
    }
}
