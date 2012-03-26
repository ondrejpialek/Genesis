using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Office.Interop.Excel;
using System.Collections.ObjectModel;

namespace Genesis.Excel
{
    public class ExcelFile : IExcelFile
    {
        public string Filename { get; protected set; }

        ReadOnlyCollection<ExcelWorksheet> worksheets;
        public ReadOnlyCollection<ExcelWorksheet> Worksheets
        {
            get
            {
                if (worksheets == null)
                {
                    var sheets = new List<ExcelWorksheet>();
                    foreach (var s in workbook.Worksheets)
                    {
                        Worksheet w = s as Worksheet;
                        if (w != null)
                        {
                            sheets.Add(new ExcelWorksheet(w));
                        }
                    }
                    worksheets = new ReadOnlyCollection<ExcelWorksheet>(sheets);
                }
                return worksheets;
            }
        }

        public bool Visible
        {
            get
            {
                return application.Visible;
            }
            set
            {
                application.Visible = value;
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
            workbook = application.Workbooks.Open(Filename);
        }

        public void Dispose()
        {
            workbook.Close(SaveChanges: false);
            application.Quit();
            application = null;
        }
    }
}
