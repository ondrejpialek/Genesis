using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genesis
{
    public class Frequency
    {
        public virtual int Id { get; private set; }
        public virtual Locality Locality { get; set; }
        public virtual double Value { get; set; }
        public virtual int SampleSize { get; set; }
    }
}
