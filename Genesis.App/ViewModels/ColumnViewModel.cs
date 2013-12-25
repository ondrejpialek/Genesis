using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Genesis.Excel;

namespace Genesis.ViewModels
{
    public class ColumnViewModel : PropertyChangedBase
    {
        private readonly IEnumerable<Gene> genes;

        public ColumnViewModel(string key, string name, IEnumerable<Gene> genes)
        {
            this.genes = genes;
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
                this.Set(() => Key, ref key, value);
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
                this.Set(() => Name, ref name, value);
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

                    this.Set(() => Column, ref column, value);
                    
                    //preselect gene based on name
                    var colName = Name.Substring(Name.IndexOf(":", StringComparison.InvariantCultureIgnoreCase) + 1);
                    Gene = genes.FirstOrDefault(g => g.Name.Equals(colName));

                    NotifyOfPropertyChange(() => IsTraitCol);
                    return;
                }

                this.Set(() => Column, ref column, value);
                NotifyOfPropertyChange(() => IsTraitCol);
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
                    NotifyOfPropertyChange(() => Gene);
                }
            }
        }
    }
}
