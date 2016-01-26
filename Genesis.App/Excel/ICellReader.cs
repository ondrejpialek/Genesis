using Genesis;
using Genesis.Excel;

namespace Genesis.Excel
{
    public interface ICellReader
    {
        int ColumnIndex { get; set; }
        string Name { get; }
    }

    public interface ICellReader<TEntity> : ICellReader
    {
        bool IsRequired { get; }

        bool TryRead(IExcelWorksheet worksheet, int row, out IApplicator<TEntity> applicator);
    }
}
