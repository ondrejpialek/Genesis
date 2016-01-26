using System;
using System.Diagnostics;
using Caliburn.Micro;
using Genesis.Excel;

namespace Genesis.ViewModels.Import
{
    public abstract class ColumnViewModel : PropertyChangedBase
    {
        public abstract Array Fields { get; }

        protected ColumnViewModel(int columnIndex, string name)
        {
            ExcelColumn = Alphabet.GetExcelColumn(columnIndex);
            ColumnIndex = columnIndex;
            Name = name;
        }

        private string excelColumn;
        public string ExcelColumn
        {
            get
            {
                return excelColumn;
            }
            set
            {
                this.Set(() => ExcelColumn, ref excelColumn, value);
            }
        }

        private int columnIndex;
        public int ColumnIndex
        {
            get
            {
                return columnIndex;
            }
            set
            {
                this.Set(() => ColumnIndex, ref columnIndex, value);
            }
        }

        private string name;


        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.Set(() => Name, ref name, value);
            }
        }

        public abstract ICellReader GetCellReader();

        /// <summary>
        /// A ViewModel that will be rendered in the cell editor, or a simple type such as string, for non-interactive display.
        /// </summary>
        public abstract object CellContent { get; }
    }
}
