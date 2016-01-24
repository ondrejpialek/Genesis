using System;
using System.Diagnostics;
using Caliburn.Micro;
using Genesis.Excel;

namespace Genesis.ViewModels.Import
{
    public abstract class ColumnViewModel : PropertyChangedBase
    {
        public abstract Array Fields { get; }

        protected ColumnViewModel(string excelColumn, string name)
        {
            ExcelColumn = excelColumn;
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
