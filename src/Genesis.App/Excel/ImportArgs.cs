using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesis.Excel;
using Genesis;

namespace Genesis.Excel
{
    public class ImportArgs<TEntity>
    {
        public string Filename { get; set; }

        public string WorkSheetName { get; set; }

        public IList<ICellReader<TEntity>> Columns { get; set; }

        public ImportArgs()
        {
            Columns = new List<ICellReader<TEntity>>();
        }
    }
}
