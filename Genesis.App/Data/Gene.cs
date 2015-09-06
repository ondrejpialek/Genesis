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

        public Gene(int id)
        {
            Id = id;
        }

        public Gene()
        {

        }
    }

    public class NominalGene : Gene
    {
        public virtual ICollection<Allele> Alleles { get; private set; }

        public NominalGene()
        {
            Alleles = new List<Allele>();
        }

        public NominalGene(int id) : base(id)
        {
            Alleles = new List<Allele>();
        }
    }

    public class OrdinalGene : Gene
    {
        public virtual double Min { get; set; }
        public virtual double Max { get; set; }

        public OrdinalGene(int id) : base(id)
        {

        }
        public OrdinalGene()
        {

        }
    }
}
