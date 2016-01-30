using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Genesis
{
    public class Category
    {
        public int Id { get; protected set; }

        [Required]
        public string Value { get; set; }

        public int TraitId { get; set; }
        [Required]
        public virtual NominalTrait Trait { get; set; }
    }
}