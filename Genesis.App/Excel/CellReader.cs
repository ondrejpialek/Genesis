using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesis;
using Genesis.Excel;

namespace Genesis.Excel
{
    public abstract class CellReader<TEntity, TValue> : ColumnBase, ICellReader<TEntity>
    {
        public bool IsRequired { get; protected set; }

        public bool IsKey { get; protected set; }

        protected abstract void Apply(TEntity entity, TValue value);

        protected Func<TEntity, TValue, bool> comparator;

        protected CellReader(string name, Func<TEntity, TValue, bool> comparator, bool isRequired, bool isKey) : base (name) {
            this.IsRequired = isRequired;
            this.IsKey = isKey;
            this.comparator = comparator;
        }

        public CellReader(string name, Func<TEntity, TValue, bool> comparator) : this(name, comparator, true, true) { }

        public CellReader(string name, bool isRequired = false) : this(name, null, isRequired, false) { }

        public bool TryRead(IExcelWorksheet worksheet, int row, out IApplicator<TEntity> applicator)
        {
            var cell = worksheet.GetCellValue(Column + row.ToString());
            if (IsRequired && cell.IsEmpty)
            {
                applicator = null;
                return false;
            }
            applicator = new Applicator<TEntity, TValue>(Apply, cell.GetValue<TValue>(), comparator);
            return true;
        }
    }
}
