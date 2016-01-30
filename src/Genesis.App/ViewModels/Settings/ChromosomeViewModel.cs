using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;

namespace Genesis.ViewModels.Settings
{
    public class ChromosomeViewModel : PropertyChangedBase
    {
        private readonly GenesisContext context;
        private readonly Chromosome chromosome;

        public ChromosomeViewModel(Chromosome chromosome, GenesisContext context)
        {
            this.chromosome = chromosome;
            this.context = context;
            Genes = new ObservableCollection<GeneViewModel>(chromosome.Genes.Select(g => new GeneViewModel(g, context)));
        }

        public ObservableCollection<GeneViewModel> Genes { get; protected set; }

        public string Name
        {
            get
            {
                return chromosome.Name;
            }
            set
            {
                chromosome.Name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public void AddGene()
        {
            var gene = new Gene()
            {
                Name = "NEW GENE",
            };

            chromosome.Genes.Add(gene);
            Genes.Add(new GeneViewModel(gene, context));
            context.Genes.Add(gene);
        }

        public void RemoveTrait(GeneViewModel geneViewModel)
        {
            chromosome.Genes.Remove((Gene)geneViewModel.GetTrait());
            Genes.Remove(geneViewModel);
            context.Traits.Remove(geneViewModel.GetTrait());
        }
    }
}