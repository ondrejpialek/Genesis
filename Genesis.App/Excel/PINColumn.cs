using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesis;

namespace Genesis.Excel
{
    public class PINColumn : CellReader<Mouse, string>
    {
        public PINColumn() : base("PIN", true) { }

        protected override void Apply(Mouse entity, string value)
        {
            entity.Name = value;
        }
    }
}
