using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Genesis
{
    public class FrequencyAnalysis
    {
        public virtual int Id { get; private set; }

        [Required]
        public virtual string Name { get; set; }

        [Required]
        public virtual DateTime Analyzed { get; set; }
        
        public virtual ICollection<Frequency> Frequencies { get; private set; }

        public FrequencyAnalysis()
        {
            Frequencies = new List<Frequency>();
        }
    }
}
