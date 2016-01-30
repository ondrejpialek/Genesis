using System;
using Caliburn.Micro;

namespace Genesis.ViewModels.Settings
{
    public interface ITraitViewModel
    {
        Trait GetTrait();
    }

    public abstract class TraitViewModel<TTrait> : PropertyChangedBase, ITraitViewModel
        where TTrait: Trait
    {
        protected readonly TTrait Trait;

        protected TraitViewModel(TTrait trait)
        {
            Trait = trait;
        }

        public string Name
        {
            get { return Trait.Name; }
            set
            {
                Trait.Name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public TTrait GetTrait()
        {
            return Trait;
        }

        Trait ITraitViewModel.GetTrait()
        {
            return Trait;
        }
    }
}