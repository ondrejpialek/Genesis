using System.Linq;

namespace Genesis.Excel
{
    public class GeneCellReader : CellReader<Mouse, string>
    {
        public GeneCellReader() : base("Gene") {
        }

        /// <summary>
        /// The gene this Trait represents.
        /// </summary>
        public Gene Gene
        {
            get;
            set;
        }

        protected override void Apply(Mouse mouse, string value)
        {
            var alleles = mouse.Records.Where(r => r.Trait == Gene).ToList();
            foreach (var a in alleles)
            {
                mouse.Records.Remove(a);
            }

            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var newAlleles = (from str in value.Split('/')
                let s = str.Trim().ToLowerInvariant()
                join allele in Gene.Categories on s equals allele.Value.ToLowerInvariant()
                select allele).ToList();

            foreach (var allele in newAlleles)
                mouse.Records.Add(new NominalRecord(allele, mouse));

            var requiredCount = mouse.Sex == Sex.Male ? Gene.Chromosome.Males : Gene.Chromosome.Females;

            if ((newAlleles.Count != 1) || (requiredCount <= 1)) return;

            for (var i = 0; i < requiredCount - 1; i++)
                mouse.Records.Add(new NominalRecord(newAlleles[0], mouse));
        }
    }
}