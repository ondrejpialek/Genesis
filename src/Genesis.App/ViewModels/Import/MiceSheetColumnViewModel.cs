using System;
using System.Linq;
using Genesis.Excel;

namespace Genesis.ViewModels.Import
{
    public class MiceSheetColumnViewModel : ColumnViewModel
    {
        private readonly GenesisContext context;
        private MiceField? field;
        public MiceField? Field
        {
            get
            {
                return field;
            }
            set
            {
                this.Set(() => Field, ref field, value);
                if (field == MiceField.Trait)
                {
                    //preselect Trait based on column name
                    var trait = context.Traits.FirstOrDefault(t => t.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase));
                    traitCellEditorViewModel = new TraitCellEditorViewModel(trait, context);                       
                }
                NotifyOfPropertyChange(() => CellContent);
            }
        }
        public MiceSheetColumnViewModel(int columnIndex, string name, GenesisContext context) : base(columnIndex, name)
        {
            this.context = context;
            ApplyConvention(name);
        }

        public void ApplyConvention(string name)
        {
            var miceField = name.ToEnum<MiceField>();

            if (miceField.HasValue)
            {
                Field = miceField;
                return;
            }

            var isTrait = context.Traits.Local.Any(t => t.Name.Equals(name));
            if (isTrait)
            {
                Field = MiceField.Trait;
            }
        }

        public override Array Fields => Enum.GetValues(typeof(MiceField));

        public override ICellReader GetCellReader()
        {
            ICellReader result;

            switch (field)
            {
                case MiceField.PIN:
                    result = new PINColumn();
                    break;
                case MiceField.Trait:
                    result = traitCellEditorViewModel.GetCellReader();
                    break;
                case MiceField.Sex:
                    result = new SexColumn();
                    break;
                case MiceField.Code:
                    result = new MouseLocalityCellReader(context.Localities.ToList());
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

        private TraitCellEditorViewModel traitCellEditorViewModel;

        public override object CellContent
        {
            get
            {
                switch (field)
                {
                    case MiceField.Trait:
                        return traitCellEditorViewModel;
                    case MiceField.Sex:
                    case MiceField.PIN:
                    case MiceField.Code:
                    case null:
                        return null;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}