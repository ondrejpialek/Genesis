using System;

namespace Genesis.Excel
{
    public interface IExcelCell
    {
        T GetValue<T>();
        bool IsEmpty { get; }
    }
}
