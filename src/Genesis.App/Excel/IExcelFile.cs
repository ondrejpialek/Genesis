using System;
using System.Collections.ObjectModel;

namespace Genesis.Excel
{
    public interface IExcelFile : IDisposable
    {
        string Filename { get; }
        ReadOnlyCollection<IExcelWorksheet> Worksheets { get; }
    }
}
