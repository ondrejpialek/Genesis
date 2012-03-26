using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;

namespace Genesis.Excel
{
    public class ExcelCell : IExcelCell
    {
        private Range range;

        public ExcelCell(Range range) {
            this.range = range;
        }

        public bool IsEmpty
        {
            get
            {
                return range.Value == null;
            }
        }

        public T GetValue<T>()
        {
            if (typeof(T) == typeof(string)) {
                return range.Text;
            }
            return (T)range.Value;
        }
    }
}
