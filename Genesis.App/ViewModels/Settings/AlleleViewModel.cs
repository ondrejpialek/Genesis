namespace Genesis.ViewModels.Settings
{
    public class AlleleViewModel : CategoryViewModel
    {
        private readonly Allele allele;

        public AlleleViewModel(Allele allele) : base(allele)
        {
            this.allele = allele;
        }

        public Species Species
        {
            get
            {
                return allele.Species;
            }
            set
            {
                allele.Species = value;
                NotifyOfPropertyChange(() => Species);
            }
        }
    }
}