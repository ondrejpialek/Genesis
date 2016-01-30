using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesis;

namespace Genesis.Excel
{
    public class SexColumn : CellReader<Mouse, string>
    {
        public SexColumn()
            : base("Sex", true)
        {
        }

        protected override void Apply(Mouse mouse, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                mouse.Sex = value.Trim().Equals("M", StringComparison.InvariantCultureIgnoreCase) ? Sex.Male : Sex.Female;
            }
        }
    }
}
