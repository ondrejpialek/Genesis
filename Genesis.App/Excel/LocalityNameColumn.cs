using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesis;

namespace Genesis.Excel
{
    public class LocalityNameColumn : CellReader<Locality, string>
    {
        public LocalityNameColumn() : base("Locality name", true) { }

        protected override void Apply(Locality locality, string value)
        {
            locality.Name = value;
        }
    }
}
