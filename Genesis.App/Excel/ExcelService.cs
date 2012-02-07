using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genesis.Excel
{
    public class ExcelService : IExcelService
    {
        public IExcelFile Open(string filename)
        {
            return new ExcelFile(filename);
        }
    }
}
