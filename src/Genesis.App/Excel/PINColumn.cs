using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesis;

namespace Genesis.Excel
{
    public class PINColumn : CellReader<Mouse, string>
    {
        private static Func<Mouse, string, bool> COMPARATOR = (mouse, value) => (!string.IsNullOrEmpty(value) && value.Equals(mouse.Name, StringComparison.InvariantCultureIgnoreCase));

        public PINColumn() : base("PIN", COMPARATOR, true) { }

        protected override void Apply(Mouse entity, string value)
        {
            entity.Name = value;
        }
    }
}
