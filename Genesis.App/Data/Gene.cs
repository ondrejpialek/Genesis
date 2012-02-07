using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genesis
{
    public class Gene
    {
        public virtual int Id { get; private set; }
        public virtual string Name { get; set; }
        public virtual Chromosome Chromosome { get; set; }
        public virtual ICollection<Allele> Alleles { get; private set; }

        public Gene()
        {
            Alleles = new List<Allele>();
        }

        public Gene(int id) : this() 
        {
            Id = id;
        }
    }
}
