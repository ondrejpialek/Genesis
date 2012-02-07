using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;

namespace Genesis.Excel
{
    public interface IExcelWorksheet
    {
        string Name { get; }

        event DocEvents_SelectionChangeEventHandler SelectionChanged;

        string GetCellValueAsString(string range);
        
        IExcelCell GetCellValue(string range);

        int GetRowCount();

        int GetColCount();

        void Activate();
    }
}
