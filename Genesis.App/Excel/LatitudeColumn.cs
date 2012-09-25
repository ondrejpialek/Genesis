using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesis;
using Microsoft.SqlServer.Types;
using System.Data.Entity.Spatial;

namespace Genesis.Excel
{
    public class LatitudeColumn : CellReader<Locality, string>
    {
        public LatitudeColumn() : base("Latitude") { }

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
                var longitude = locality.Location == null ? 0 : locality.Location.Longitude ?? 0;
                var point = SqlGeography.Point(value.Value, longitude, 4326);
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
