using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesis;

namespace Genesis.Excel
{
    public abstract class ColumnBase : ICellReader
    {
        public string Name { get; protected set; }

        public string Column { get; set; }

        protected ColumnBase(string name)
        {
            Name = name;
        }
    }
}
