namespace Genesis.ViewModels.Settings
{
    public class GeneViewModel : NominalTraitViewModel
    {
        private readonly Gene gene;

        public GeneViewModel(Gene gene, GenesisContext context) : base(gene, context)
        {
            this.gene = gene;
        }

        public int StartBasePair
        {
            get { return gene.StartBasePair; }
            set
            {
                gene.StartBasePair = value;
                NotifyOfPropertyChange(() => StartBasePair);
            }
        }

        protected override CategoryViewModel CreateNewCategory()
        {
            var allele = new Allele()
            {
                Value = "NEW CATEGORY"
            };
            return new AlleleViewModel(allele);
        }
    }
}