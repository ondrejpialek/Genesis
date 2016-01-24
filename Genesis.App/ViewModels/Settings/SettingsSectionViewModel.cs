using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using Caliburn.Micro;

namespace Genesis.ViewModels.Settings
{
    public class SettingsSectionViewModel : SectionViewModel
    {                   

        public SettingsSectionViewModel()
        {
            DisplayName = "Settings";
            Order = 100;

            var method = ActionMessage.GetTargetMethod;
            ActionMessage.GetTargetMethod = (message, o) =>
            {
                return method(message, o);
            };

            if (Execute.InDesignMode)
            {
                context = new GenesisContext();
                context.Species.Load();
            }
        }

        private GenesisContext context;

        protected override void OnActivate()
        {
            base.OnActivate();

            context?.Dispose();
            context = new GenesisContext();

            context.Species.Load();

            traits = null;
            NotifyOfPropertyChange(() => Traits);
            NotifyOfPropertyChange(() => Species);
        }

        private ObservableCollection<PropertyChangedBase> traits;
        public ObservableCollection<PropertyChangedBase> Traits
        {
            get
            {
                if (context == null)
                    return null;

                if (traits == null)
                {
                    var chromosomes = context.Chromosomes
                       .ToList()
                       .Select(ch => new ChromosomeViewModel(ch, context));

                    var nonGeneTraits = context.Traits
                        .Where(t => !(t is Gene))
                        .ToList()
                        .Select(t => WrapTraitIntoModel(t, context));

                    traits = new ObservableCollection<PropertyChangedBase>(chromosomes.Concat(nonGeneTraits));
                }

                return traits;
            }
        }

        public ObservableCollection<Species> Species
        {
            get
            {
                if (context == null)
                    return null;

                return context.Species.Local;
            }
        }

        public void AddSpecies()
        {
            context.Species.Add(new Species()
            {
                Name = "NEW SPECIES"
            });
        }

        public void RemoveSpecies(Species species)
        {
            context.Species.Remove(species);
        }

        //TODO: might be a command, also send event
        public void RemoveTrait(ITraitViewModel traitViewModel)
        {
            var trait = traitViewModel.GetTrait();
            if (trait is Gene)
            {
                throw new NotSupportedException("This should be handled by the chromosome view model.");
            }

            //might need to delete categories?
            var nominalTrait = trait as NominalTrait;
            if (nominalTrait != null)
            {
                context.Categories.RemoveRange(nominalTrait.Categories);
            }

            Traits.Remove((PropertyChangedBase)traitViewModel);
            context.Traits.Remove(traitViewModel.GetTrait());
        }

        //TODO: might be a command
        public void AddOrdinalTrait()
        {
            var trait = new OrdinalTrait() {Name = "New Ordinal Trait"};
            context.Traits.Add(trait);
            Traits.Add(WrapTraitIntoModel(trait, context));
        }

        //TODO: might be a command
        public void AddNominalTrait()
        {
            var trait = new NominalTrait() {Name = "New Nominal Trait"};
            context.Traits.Add(trait);
            Traits.Add(WrapTraitIntoModel(trait, context));
        }

        public void Recreate()
        {
            context.Database.Delete();
        }

        public void Reset()
        {
            var localities = context.Localities.ToList();
            foreach (var l in localities)
                context.Localities.Remove(l);
            context.SaveChanges();
        }

        public void Save()
        {
            try
            {
                context.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var err in e.EntityValidationErrors)
                {
                    foreach (var msg in err.ValidationErrors)
                    {
                        Console.WriteLine("{1} ({0}): {2} - {3}", err.Entry.Entity.GetType(), err.Entry.Entity, msg.PropertyName, msg.ErrorMessage);
                    }
                }
                throw;
            }
        }

        private static PropertyChangedBase WrapTraitIntoModel(Trait trait, GenesisContext context)
        {
            var ordinal = trait as OrdinalTrait;
            if (ordinal != null)
                return new OrdinalTraitViewModel(ordinal);

            var nominal = trait as NominalTrait;
            if (nominal != null)
            {
                return new NominalTraitViewModel(nominal, context);
            }

            throw new NotSupportedException("Unknown trait type.");
        }
    }
}