using GalaSoft.MvvmLight;
using Genesis.Excel;

namespace Genesis.ViewModels
{
    public class ColumnViewModel : ViewModelBase
    {

        public ColumnViewModel(string key, string name)
        {
            Key = key;
            Name = name;
        }

        
        private string key = null;
        public string Key
        {
            get
            {
                return key;
            }
            set
            {
                Set(() => Key, ref key, value);
            }
        }

        
        private string name = null;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                Set(() => Name, ref name, value);
            }
        }

        private ICellReader column;
        public ICellReader Column
        {
            get { return column; }
            set
            {
                if (value is TraitColumn) {
                    if (column is TraitColumn)
                        return;

                    value = new TraitColumn();
                }

                Set(() => Column, ref column, value);
                RaisePropertyChanged(() => IsTraitCol);
            }
        }

        public bool IsTraitCol
        {
            get
            {
                return Column is TraitColumn;
            }
        }

        public Gene Gene
        {
            get
            {
                var traitCol = column as TraitColumn;
                if (traitCol == null)
                    return null;
                else
                    return traitCol.Gene;
            }
            set
            {
                var traitCol = column as TraitColumn;
                if (traitCol != null)
                {
                    traitCol.Gene = value;
                    RaisePropertyChanged(() => Gene);
                }
            }
        }
    }
}
