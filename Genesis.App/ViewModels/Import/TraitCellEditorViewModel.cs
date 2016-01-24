using System;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Genesis.Excel;

namespace Genesis.ViewModels.Import
{
    public class TraitCellEditorViewModel : PropertyChangedBase
    {
        private readonly GenesisContext context;

        public TraitCellEditorViewModel(Trait preselctedValue, GenesisContext context)
        {
            this.context = context;
            Trait = preselctedValue;
        }

        private Trait trait;
        public Trait Trait
        {
            get { return trait; }
            set
            {
                trait = value;
                NotifyOfPropertyChange(() => Trait);
            }
        }

        public ObservableCollection<Trait> Traits
        {
            get { return context.Traits.Local; }
        } 

        public ICellReader GetCellReader()
        {
            if (trait == null)
                return null;

            var gene = trait as Gene;
            if (gene != null)
            {
                return new GeneCellReader()
                {
                    Gene = gene
                };
            }

            var ordinalTrait = trait as OrdinalTrait;
            if (ordinalTrait != null)
            {
                return new OrdinalTraitCellReader();
            }

            var nominalTrait = trait as NominalTrait;
            if (nominalTrait != null)
            {
                return new NominalTraitCellReader();
            }


            throw new NotImplementedException();
            

        }
    }
}