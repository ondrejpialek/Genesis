using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Genesis
{
    public abstract class Chromosome
    {
        #region properties
        public virtual int Id { get; private set; }
        [Required]
        public virtual string Name { get; set; }

        [Required]
        public abstract int Males { get; }
        [Required]
        public abstract int Females { get; }

        public virtual ICollection<Gene> Genes { get; private set; }
        #endregion

        public Chromosome()
        {
            Genes = new List<Gene>();
        }
    }

    public class XChromosome : Chromosome
    {
        public override int Males { get { return 1; } }
        public override int Females { get { return 2; } }
    }

    public class YChromosome : Chromosome
    {
        public override int Males { get { return 1; } }
        public override int Females { get { return 0; } }
    }

    public class MtDNAChromosome : Chromosome
    {
        public override int Males { get { return 1; } }
        public override int Females { get { return 1; } }
    }

    public class AutosomalChromosome : Chromosome
    {
        public override int Males { get { return 2; } }
        public override int Females { get { return 2; } }
    }
}
