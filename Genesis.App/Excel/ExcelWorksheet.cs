using System;
using System.Linq;
using Microsoft.Office.Interop.Excel;

namespace Genesis.Excel
{
    [Obsolete("", true)]
    public class ExcelWorksheet : IExcelWorksheet
    {
        public string Name => worksheet.Name;

        private readonly Worksheet worksheet;

        public ExcelWorksheet(Worksheet worksheet)
        {
            this.worksheet = worksheet;
        }

        public string GetCellValueAsString(string range)
        {
            var r = worksheet.get_Range(range);
            dynamic value = r.Value;
            return value == null ? string.Empty : value.ToString();
        }

        protected IExcelCell GetCellValue(string range)
        { 
            return new ExcelCell(worksheet.Range[range]);
        }

        public string[] GetColumns()
        {
            return Enumerable.Range(0, GetColCount()).Select(i => GetCellValue(i, 0).GetValue<string>()).ToArray();
        }

        public int GetRowCount()
        {
            var range = worksheet.UsedRange;
            var rows = range.Rows;
            return rows.Count;
        }

        public int GetColCount()
        {
            var range = worksheet.UsedRange;
            var cols = range.Columns;
            return cols.Count;
        }

        public IExcelCell GetCellValue(int column, int row)
        {
            return GetCellValue(Alphabet.GetExcelColumn(column) + (row + 1)); // rows in excel are 1-based
        }
    }
}
