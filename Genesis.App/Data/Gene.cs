using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Genesis
{
    public class Gene
    {
        public virtual int Id { get; private set; }
        public virtual string Name { get; set; }
        public virtual Chromosome Chromosome { get; set; }
        public virtual ObservableCollection<Allele> Alleles { get; private set; }

        public Gene()
        {
            Alleles = new ObservableCollection<Allele>();
        }

        public Gene(int id) : this() 
        {
            Id = id;
        }
    }
}
