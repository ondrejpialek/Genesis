using System.Collections.ObjectModel;
using System.Linq;

namespace Genesis.ViewModels.Settings
{
    public class NominalTraitViewModel : TraitViewModel<NominalTrait>
    {
        private readonly GenesisContext context;

        public NominalTraitViewModel(NominalTrait trait, GenesisContext context) : base(trait)
        {
            this.context = context;
            Categories = new ObservableCollection<CategoryViewModel>(trait.Categories.Select(CategoryViewModel.WrapCategoryInModel));
        }

        public ObservableCollection<CategoryViewModel> Categories { get; protected set; }

        protected virtual CategoryViewModel CreateNewCategory()
        {
            var allele = new Category
            {
                Value = "NEW CATEGORY"
            };
            return new CategoryViewModel(allele);
        }

        public void AddCategory()
        {
            var categoryViewModel = CreateNewCategory();
            Trait.Categories.Add(categoryViewModel.GetCategory());
            Categories.Add(categoryViewModel);
        }

        public void RemoveCategory(CategoryViewModel category)
        {
            Categories.Remove(category);
            Trait.Categories.Remove(category.GetCategory());
            context.Categories.Remove(category.GetCategory());
        }
    }
}