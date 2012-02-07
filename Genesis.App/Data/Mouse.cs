using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genesis
{
    public class Mouse
    {
        public virtual int Id { get; private set; }
        public virtual String Name { get; set; }
        public virtual Sex Sex { get; set; }
        public virtual Locality Locality { get; set; }
        public virtual ICollection<AlleleRecord> Alleles { get; private set; }

        public Mouse()
        {
            Alleles = new List<AlleleRecord>();
        }
    }
}
