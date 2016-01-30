using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.IO;

namespace Genesis.Excel
{
    public class DataTableExcelFile : IExcelFile
    {
        private OleDbConnection connection;

        public DataTableExcelFile(string filename)
        {
            if (!File.Exists(filename))
                throw new ArgumentException("The filename must exist.");

            Filename = filename;
            var connectionString =
                $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filename};"+
                "Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\"";

            connection = new OleDbConnection(connectionString);
            GetSheets();
        }

        private void GetSheets()
        {     
            connection.Open();
            var sheets = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            var workseets = new List<IExcelWorksheet>();
            for(var rowIndex = 0; rowIndex < sheets.Rows.Count; rowIndex++)
            {
                var row = sheets.Rows[rowIndex];
                var sheetName = row["TABLE_NAME"] as string;
                sheetName = sheetName?.Replace("$", ""); // the sheet names end with a dollar sign
                workseets.Add(new DataTableExcelWorksheet(sheetName, connection));
                Worksheets = new ReadOnlyCollection<IExcelWorksheet>(workseets);
            }
        }

        public void Dispose()
        {
            connection?.Dispose();
            connection = null;
        }

        public string Filename { get; }
        public ReadOnlyCollection<IExcelWorksheet> Worksheets { get; private set; }
    }
}