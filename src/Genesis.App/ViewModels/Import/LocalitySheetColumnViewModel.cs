using System;
using System.Linq;
using Genesis.Excel;

namespace Genesis.ViewModels.Import
{
    public class LocalitySheetColumnViewModel : ColumnViewModel
    {

        private LocalityField? field;

        public override Array Fields => Enum.GetValues(typeof(LocalityField));

        public LocalitySheetColumnViewModel(int columnIndex, string name) : base(columnIndex, name)
        {
            ApplyConvention(name);
        }

        public LocalityField? Field
        {
            get { return field; }
            set
            {
                this.Set(() => Field, ref field, value);
                NotifyOfPropertyChange(() => CellContent);
            }
        }

        /// <returns>No editor needed, just returns the name of the selected field, if any.</returns>
        public override object CellContent => null;

        public override ICellReader GetCellReader()
        {
            ICellReader result;

            switch (field)
            {
                case LocalityField.Code:
                    result = new LocalityCodeCellReader();
                    break;
                case LocalityField.Latitude:
                    result = new LatitudeLocationComponentCellReader();
                    break;
                case LocalityField.Longitude:
                    result = new LongitudeLocationComponentCellReader();
                    break;
                case LocalityField.Name:
                    result = new LocalityNameCellReader();
                    break;
                case null:
                    result = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (result != null)
            {
                result.ColumnIndex = ColumnIndex;
            }

            return result;
        }

        public void ApplyConvention(string name)
        {
            Field = name.ToEnum<LocalityField>();
        }
    }
}