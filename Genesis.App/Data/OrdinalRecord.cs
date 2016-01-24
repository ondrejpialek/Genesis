using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Genesis
{
    public class OrdinalRecord : Record
    {
        [Required]
        public double Value { get; set; }

        public OrdinalRecord(OrdinalTrait trait, Mouse mouse) : base(trait, mouse) { }
        public OrdinalRecord() { }

        [NotMapped]
        public OrdinalTrait OrdinalTrait => (OrdinalTrait) Trait;
    }
}