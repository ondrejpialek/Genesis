using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Genesis
{
    public abstract class Chromosome
    {
        #region properties
        public virtual int Id { get; private set; }
        public virtual string Name { get; set; }

        public abstract int Males { get; }
        public abstract int Females { get; }

        public virtual ObservableCollection<Gene> Genes { get; private set; }
        #endregion

        public Chromosome()
        {
            Genes = new ObservableCollection<Gene>();
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
