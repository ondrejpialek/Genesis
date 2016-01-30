using Caliburn.Micro;

namespace Genesis.ViewModels.Settings
{
    public class CategoryViewModel : PropertyChangedBase
    {
        protected Category Category;

        public CategoryViewModel(Category category)
        {
            Category = category;
        }

        public string Value
        {
            get
            {
                return Category.Value;
            }
            set
            {
                Category.Value = value;
                NotifyOfPropertyChange(() => Value);
            }
        }

        public Category GetCategory()
        {
            return Category;
        }

        public static CategoryViewModel WrapCategoryInModel(Category category)
        {
            var allele = category as Allele;
            return allele != null ? new AlleleViewModel(allele) : new CategoryViewModel(category);
        }
    }
}