using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesis;

namespace Genesis.Excel
{
    public class TraitColumn : CellReader<Mouse, string>
    {
        public TraitColumn() : base("Trait", true) {
        }

        public Gene Gene
        {
            get;
            set;
        }

        protected override void Apply(Mouse mouse, string value)
        {
            var alleles = mouse.Alleles.Where(a => a.Allele.Gene == Gene).ToList();
            foreach(var a in alleles) {
                mouse.Alleles.Remove(a);
            }

            var newAlleles = (from str in value.Split('/')
                              join allele in Gene.Alleles on str equals allele.Value
                              select allele).ToList();

            foreach(var allele in newAlleles)
                mouse.Alleles.Add(new AlleleRecord(allele));

            var requiredCount = mouse.Sex == Sex.Male ? Gene.Chromosome.Males : Gene.Chromosome.Females;

            if ((newAlleles.Count == 1) && (requiredCount > 1))
            {
                for (int i = 0; i < requiredCount - 1; i++)
                    mouse.Alleles.Add(new AlleleRecord(newAlleles[0]));
            }
        }
    }
}
