using System.Collections.Generic;

namespace Genesis.Excel
{
    internal class WorksheetReader<TEntity>
    {
        private readonly IExcelWorksheet worksheet;
        private readonly RowReader<TEntity> rowReader;
        private readonly int skip;

        public WorksheetReader(IExcelWorksheet worksheet, RowReader<TEntity> rowReader, int skip = 1)
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
                for (int i = 0 + skip; i < rowsCount; i++)
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
