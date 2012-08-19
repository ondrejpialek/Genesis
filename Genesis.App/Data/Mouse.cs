using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Genesis
{
    public class Mouse
    {
        public virtual int Id { get; private set; }
        [Required]
        public virtual String Name { get; set; }
        [Required]
        public virtual Sex Sex { get; set; }
        [Required]
        public virtual Locality Locality { get; set; }

        public virtual ICollection<AlleleRecord> Alleles { get; private set; }

        public Mouse()
        {
            Alleles = new List<AlleleRecord>();
        }
    }
}
