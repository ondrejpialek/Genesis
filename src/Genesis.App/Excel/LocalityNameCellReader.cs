using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesis;

namespace Genesis.Excel
{
    /// <summary>
    /// Assignes the parsed value to the <see cref="Locality.Name" /> property.
    /// </summary>
    public class LocalityNameCellReader : CellReader<Locality, string>
    {
        public LocalityNameCellReader() : base("Locality name", true) { }

        protected override void Apply(Locality locality, string value)
        {
            locality.Name = value;
        }
    }
}
