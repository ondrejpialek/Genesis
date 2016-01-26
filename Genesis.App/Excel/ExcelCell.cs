using System;
using Microsoft.Office.Interop.Excel;

namespace Genesis.Excel
{
    [Obsolete("", true)]
    public class ExcelCell : IExcelCell
    {
        private readonly Range range;

        public ExcelCell(Range range) {
            this.range = range;
        }

        public bool IsEmpty => range.Value == null;

        public T GetValue<T>()
        {
            if (typeof(T) == typeof(string)) {
                return range.Text;
            }
            return (T)range.Value;
        }
    }
}
