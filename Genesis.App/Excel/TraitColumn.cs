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

            var newAlleles = value.Split('/');
            foreach(var newAllele in newAlleles) {
                var allele = Gene.Alleles.FirstOrDefault(a => string.Equals(a.Value, newAllele));

                if (allele != null)
                {
                    mouse.Alleles.Add(new AlleleRecord(allele));
                }
            }
        }
    }
}
