using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Genesis
{
    public class AlleleRecord
    {
        public AlleleRecord()
        {

        }

        public AlleleRecord(Allele allele)
        {
            Allele = allele;
        }

        public virtual int Id { get; private set; }

        [Required]
        public virtual Allele Allele { get; set; }
    }
}
