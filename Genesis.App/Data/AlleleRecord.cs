using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        [Key, Column(Order = 1), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; private set; }

        [Key, Column(Order = 2), ForeignKey("Mouse")]
        public int Mouse_Id { get; set; }

        [Required]
        public virtual Mouse Mouse { get; set; }

        [ForeignKey("Allele")]
        public int Allele_Id { get; set; }

        [Required]
        public virtual Allele Allele { get; set; }
    }
}
