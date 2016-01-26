using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesis;

namespace Genesis.Excel
{
    public abstract class CellReader : ICellReader
    {
        public string Name { get; protected set; }

        public int ColumnIndex { get; set; }

        protected CellReader(string name)
        {
            Name = name;
        }
    }
}
