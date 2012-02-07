using System;
using Genesis;

namespace Genesis.Excel
{
    public class CodeColumn : CellReader<Locality, string>
    {
        private static Func<Locality, string, bool> COMPARATOR = (locality, value) => !string.IsNullOrEmpty(value) && value.Equals(locality.Code);

        public CodeColumn() : base("Code", COMPARATOR) { }

        protected override void Apply(Locality locality, string value)
        {
            locality.Code = value;
        }
    }
}
