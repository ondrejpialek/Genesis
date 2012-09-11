using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Genesis
{
    public class Gene
    {
        public virtual int Id { get; private set; }
        [Required]
        public virtual string Name { get; set; }
        [Required]
        public virtual Chromosome Chromosome { get; set; }

        public virtual int StartBasePair { get; set; }

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
