using System;
using System.Data;

namespace Genesis.Excel
{
    public class DataRowExcelCell : IExcelCell
    {
        private readonly DataRow row;
        private readonly int columnIndex;
        public DataRowExcelCell(DataRow row, int columnIndex)
        {
            this.row = row;
            this.columnIndex = columnIndex;
        }

        public T GetValue<T>()
        {
            var value = row[columnIndex];
            if (value != DBNull.Value)
                return (T)value;
            return default(T);
        }

        public bool IsEmpty
        {
            get { return row.IsNull(columnIndex); }
        }
    }
}