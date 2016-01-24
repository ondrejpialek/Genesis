using System.ComponentModel.DataAnnotations;

namespace Genesis
{
    public class Category
    {
        public virtual int Id { get; private set; }

        [Required]
        public string Value { get; set; }

        [Required]
        public virtual NominalTrait Trait { get; set; }
    }
}