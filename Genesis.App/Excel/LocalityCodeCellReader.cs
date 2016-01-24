using System;
using Genesis;

namespace Genesis.Excel
{
    /// <summary>
    /// Assignes the parsed value to the <see cref="Locality.Code" /> property.
    /// </summary>
    public class LocalityCodeCellReader : CellReader<Locality, string>
    {
        private static Func<Locality, string, bool> COMPARATOR = (locality, value) => !string.IsNullOrEmpty(value) && value.Equals(locality.Code);

        public LocalityCodeCellReader() : base("Code", COMPARATOR) { }

        protected override void Apply(Locality locality, string value)
        {
            locality.Code = value;
        }
    }
}
