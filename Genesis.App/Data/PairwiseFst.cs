using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genesis.Data
{
    public class PairwiseFst
    {
        public virtual int Id { get; private set; }
        public virtual Locality Locality1 { get; set; }
        public virtual Locality Locality2 { get; set; }
        public virtual double Fst { get; set; }
        public virtual double Distance { get; set; }
    }
}
