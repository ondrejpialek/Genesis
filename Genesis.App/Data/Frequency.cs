using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Genesis
{
    public class Frequency
    {
        public virtual int Id { get; private set; }
        [Required]
        public virtual Locality Locality { get; set; }
        [Required]
        public virtual string Name { get; set; }
        [Required]
        public virtual double Value { get; set; }
        [Required]
        public virtual int SampleSize { get; set; }
    }
}
