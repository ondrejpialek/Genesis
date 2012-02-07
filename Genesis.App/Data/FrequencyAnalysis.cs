using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genesis
{
    public class FrequencyAnalysis
    {
        public virtual int Id { get; private set; }
        public virtual String Name { get; set; }
        public virtual DateTime Analyzed { get; set; }
        
        public virtual ICollection<Frequency> Frequencies { get; private set; }

        public FrequencyAnalysis()
        {
            Frequencies = new List<Frequency>();
        }
    }
}
