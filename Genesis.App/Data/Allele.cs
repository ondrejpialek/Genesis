using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Genesis
{
    public class Allele : Category
    {
        [NotMapped]
        public virtual Gene Gene {
            get
            {
                return (Gene) Trait;
            }
            set { Trait = value; }
        }

        [Required]
        public virtual Species Species { get; set; }
    }
}