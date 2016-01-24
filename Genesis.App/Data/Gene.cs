using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Genesis
{
    public class Gene : NominalTrait
    {
        [Required]
        public virtual Chromosome Chromosome { get; set; }

        public virtual int StartBasePair { get; set; }
    }
}
