using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesis.Data;

namespace Genesis
{
    public class FstAnalysis
    {
        public virtual int Id { get; private set; }
        public virtual String Name { get; set; }
        public virtual DateTime Analyzed { get; set; }

        public virtual double Fst { get; set; }

        public virtual ICollection<PairwiseFst> Pairwise { get; private set; }
        public virtual ICollection<Locality> SubPopulation1 { get; private set; }
        public virtual ICollection<Locality> SubPopulation2 { get; private set; }

        public FstAnalysis()
        {
            Pairwise = new List<PairwiseFst>();
            SubPopulation1 = new List<Locality>();
            SubPopulation2 = new List<Locality>();
        }
    }
}
