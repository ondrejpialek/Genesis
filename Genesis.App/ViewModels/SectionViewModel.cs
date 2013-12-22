using Caliburn.Micro;

namespace Genesis.ViewModels
{
    public abstract class SectionViewModel : Screen, ISectionViewModel
    {
        public int Order { get; protected set; }
    }
}