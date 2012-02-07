using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genesis;

namespace Genesis.Excel
{
    internal class WorksheetReader<TEntity>
    {
        private ExcelWorksheet worksheet;
        private RowReader<TEntity> rowReader;
        private int skip;

        public WorksheetReader(ExcelWorksheet worksheet, RowReader<TEntity> rowReader, int skip = 1)
        {
            this.worksheet = worksheet;
            this.rowReader = rowReader;
            this.skip = skip;
        }

        public int GetRecordCount()
        {
            return worksheet.GetRowCount() - skip;
        }

        public IEnumerable<RowApplicator<TEntity>> Records
        {
            get {
                int rowsCount = worksheet.GetRowCount();
                for (int i = 1 + skip; i <= rowsCount; i++)
                { 
                    RowApplicator<TEntity> applicator;
                    if (rowReader.TryRead(worksheet, i, out applicator))
                    {
                        yield return applicator;
                    }
                }
            }
        }
    }
}
