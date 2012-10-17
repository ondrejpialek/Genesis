using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;
using Microsoft.CSharp;

namespace Genesis.Excel
{
    public class ExcelWorksheet : IExcelWorksheet
    {
        public string Name {
            get {
                return worksheet.Name;
            }
        }

        public event DocEvents_SelectionChangeEventHandler SelectionChanged;

        private Worksheet worksheet;

        public ExcelWorksheet(Worksheet worksheet)
        {
            this.worksheet = worksheet;
            this.worksheet.SelectionChange += new DocEvents_SelectionChangeEventHandler(worksheet_SelectionChange);
        }

        void worksheet_SelectionChange(Range Target)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged.Invoke(Target);
            }
        }

        public string GetCellValueAsString(string range)
        {
            Range r = worksheet.get_Range(range);
            dynamic value = r.Value;
            if (value == null)
                return string.Empty;
            else
                return value.ToString();
        }

        public IExcelCell GetCellValue(string range)
        { 
            return new ExcelCell(worksheet.Range[range]);
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

        public void Activate() {
            worksheet.Activate();
        }
    }
}
