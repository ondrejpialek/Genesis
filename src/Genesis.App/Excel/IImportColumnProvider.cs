using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesis.Excel;
using Genesis;

namespace Genesis.Excel
{
    public interface IImportColumnProvider<TEntity>
    {
        IEnumerable<ICellReader<TEntity>> GetColumns();
    }
}
