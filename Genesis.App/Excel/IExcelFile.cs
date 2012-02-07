using System;
using System.Collections.ObjectModel;

namespace Genesis.Excel
{
    public interface IExcelFile : IDisposable
    {
        string Filename { get; }
        bool Visible { get; set; }
        ReadOnlyCollection<ExcelWorksheet> Worksheets { get; }
    }
}
