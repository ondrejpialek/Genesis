using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesis.Data;
using System.ComponentModel.DataAnnotations;

namespace Genesis
{
    public class FstAnalysis
    {
        public virtual int Id { get; private set; }

        [Required]
        public virtual String Name { get; set; }
        [Required]
        public virtual DateTime Analyzed { get; set; }

        [Required]
        public virtual double Fst { get; set; }
    }
}
