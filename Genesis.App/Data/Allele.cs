using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Genesis
{
    public class Allele
    {
        public virtual int Id { get; private set; }
        [Required]
        public virtual string Value { get; set; }
        [Required]
        public virtual Gene Gene { get; set; }
        [Required]
        public virtual Species Species { get; set; }
    }
}
