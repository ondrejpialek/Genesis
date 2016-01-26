using System;

namespace Genesis.Excel
{
    public abstract class CellReader<TEntity, TValue> : CellReader, ICellReader<TEntity>
    {
        public bool IsRequired { get; protected set; }

        protected abstract void Apply(TEntity entity, TValue value);

        protected Func<TEntity, TValue, bool> Comparator;

        protected CellReader(string name, Func<TEntity, TValue, bool> comparator, bool isRequired) : base (name) {
            IsRequired = isRequired;
            Comparator = comparator;
        }

        protected CellReader(string name, Func<TEntity, TValue, bool> comparator) : this(name, comparator, true) { }

        protected CellReader(string name, bool isRequired = false) : this(name, null, isRequired) { }

        public bool TryRead(IExcelWorksheet worksheet, int row, out IApplicator<TEntity> applicator)
        {
            var cell = worksheet.GetCellValue(ColumnIndex, row);
            if (IsRequired && cell.IsEmpty)
            {
                applicator = null;
                return false;
            }
            applicator = new Applicator<TEntity, TValue>(Apply, cell.GetValue<TValue>(), Comparator);
            return true;
        }
    }
}
