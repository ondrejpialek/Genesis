namespace Genesis.Excel
{
    public interface IExcelWorksheet
    {
        string Name { get; }

        /// <summary>
        /// Get the column names in the worksheet.
        /// </summary>
        string[] GetColumns();

        int GetRowCount();

        int GetColCount();

        IExcelCell GetCellValue(int column, int row);
    }
}
