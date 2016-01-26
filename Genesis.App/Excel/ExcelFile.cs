using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Office.Interop.Excel;
using System.Collections.ObjectModel;

namespace Genesis.Excel
{
    [Obsolete("", true)]
    public class ExcelFile : IExcelFile
    {
        public string Filename { get; protected set; }

        ReadOnlyCollection<IExcelWorksheet> worksheets;
        public ReadOnlyCollection<IExcelWorksheet> Worksheets
        {
            get
            {
                if (worksheets == null)
                {
                    var sheets = new List<IExcelWorksheet>();

                        foreach (var s in workbook.Worksheets)
                        {
                            Worksheet w = s as Worksheet;
                            if (w != null)
                            {
                                sheets.Add(new ExcelWorksheet(w));
                            }
                        }
                    worksheets = new ReadOnlyCollection<IExcelWorksheet>(sheets);
                }
                return worksheets;
            }
        }

        private Application application;
        private Workbook workbook;

        public ExcelFile(string filename)
        {
            Filename = filename;
            Open();
        }

        private void Open()
        {
            if (!File.Exists(Filename))
                throw new ArgumentException("The filename must exist.");
            application = new Application();
            var workbooks = application.Workbooks;
            workbook = workbooks.Open(Filename);
        }

        public void Dispose()
        {
            workbook.Close(SaveChanges: false);
            application.Quit();
            application = null;
        }
    }
}
