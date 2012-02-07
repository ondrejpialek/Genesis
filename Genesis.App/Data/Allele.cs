using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genesis
{
    public class Allele
    {
        public virtual int Id { get; private set; }
        public virtual string Value { get; set; }
        public virtual Gene Gene { get; set; }
        public virtual Species Species { get; set; }
    }
}
