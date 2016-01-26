using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;


namespace Genesis.Excel
{
    /*

private static DataTable ConvertExcelFileToDataTable(string excelFileName)
{
string connectionString =
String.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\"", excelFileName);

using (OleDbConnection connection = new OleDbConnection(connectionString))
{
string query = "SELECT * FROM [Sheet1$]";
      connection.Open();
      OleDbCommand command = new OleDbCommand(query, connection);
      OleDbDataAdapter adapter = new OleDbDataAdapter(command);
      DataTable excelTable = new DataTable();
      dapter.Fill(excelTable);
      return excelTable;
}
}


*/
    public class DataTableExcelWorksheet : IExcelWorksheet
    {
        private readonly OleDbConnection connection;
        private DataTable dataTable;

        public DataTableExcelWorksheet(string name, OleDbConnection connection)
        {
            this.connection = connection;
            Name = name;
        }

        public string Name { get; }

        public string[] GetColumns()
        {
            string query = $"SELECT TOP 1 * FROM [{Name}$]";
            var command = new OleDbCommand(query, connection);
            var adapter = new OleDbDataAdapter(command);
            var excelTable = new DataTable();
            adapter.Fill(excelTable);
            var list = new string[excelTable.Columns.Count];
            for (var i = 0; i < excelTable.Columns.Count; i++)
            {
                //list[i] = excelTable.Rows[0].IsNull(i) ? null : excelTable.Rows[0][i].ToString();
                list[i] = excelTable.Columns[i].ColumnName;
            }
            return list;
        }

        public int GetRowCount()
        {
            EnsureDataTable();
            return dataTable.Rows.Count;
        }

        public int GetColCount()
        {
            EnsureDataTable();
            return dataTable.Columns.Count;
        }

        public IExcelCell GetCellValue(int column, int row)
        {
            EnsureDataTable();
            return new DataRowExcelCell(dataTable.Rows[row], column);
        }

        private void EnsureDataTable()
        {
            if (dataTable == null)
            {
                string query = $"SELECT * FROM [{Name}$]";
                var command = new OleDbCommand(query, connection);
                var adapter = new OleDbDataAdapter(command);
                dataTable = new DataTable();
                adapter.Fill(dataTable);
            }
        }
    }
}