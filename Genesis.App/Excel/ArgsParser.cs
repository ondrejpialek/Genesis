using System.Collections.Generic;
using System.Linq;
using Genesis;
using Genesis.Excel;

namespace Genesis.Excel
{
    public class ArgsParser<TEntity>
    {
        private ImportArgs<TEntity> args;

        public ArgsParser(ImportArgs<TEntity> args)
        {
            this.args = args;
        }

        public RowReader<TEntity> GetRowReader()
        {
            return new RowReader<TEntity>(args.Columns);
        }

        public ExcelFile GetExcelFile()
        {
            return new ExcelFile(args.Filename);
        }

        public ExcelWorksheet GetExcelWorksheet(ExcelFile file)
        {
            return file.Worksheets.First(x => x.Name == args.WorkSheetName);
        }
    }
}
