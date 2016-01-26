using System.Linq;

namespace Genesis.Excel
{
    public class ArgsParser<TEntity>
    {
        private readonly ImportArgs<TEntity> args;

        public ArgsParser(ImportArgs<TEntity> args)
        {
            this.args = args;
        }

        public RowReader<TEntity> GetRowReader()
        {
            return new RowReader<TEntity>(args.Columns);
        }

        public IExcelFile GetExcelFile()
        {
            return new DataTableExcelFile(args.Filename);
        }

        public IExcelWorksheet GetExcelWorksheet(IExcelFile file)
        {
            return file.Worksheets.First(x => x.Name == args.WorkSheetName);
        }
    }
}
