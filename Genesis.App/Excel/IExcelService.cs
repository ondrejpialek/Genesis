using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genesis.Excel
{
    public interface IExcelService
    {
        IExcelFile Open(string filename);
    }
}
