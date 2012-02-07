using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesis;

namespace Genesis.Excel
{
    public class RowReader<TEntity>
    {
        public IEnumerable<ICellReader<TEntity>> CellReaders { get; protected set; }

        public RowReader(IEnumerable<ICellReader<TEntity>> cellReaders)
        {
            CellReaders = new List<ICellReader<TEntity>>(cellReaders);
        }

        public bool TryRead(IExcelWorksheet worksheet, int row, out RowApplicator<TEntity> rowApplicator)
        {
            rowApplicator = null;

            var applicators = new List<IApplicator<TEntity>>();
            foreach (var cellReader in CellReaders)
            {
                IApplicator<TEntity> applicator;
                if (cellReader.TryRead(worksheet, row, out applicator))
                {
                    applicators.Add(applicator);
                } 
                else if (cellReader.IsRequired)
                {
                    return false;
                }    
            }
            rowApplicator = new RowApplicator<TEntity>(applicators);
            return true;
        }
    }
}
