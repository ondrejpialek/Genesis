using System.ComponentModel.DataAnnotations;

namespace Genesis
{
    public class NominalRecord : Record
    {

        [Required]
        public virtual Category Category { get; set; }

        public NominalRecord(Category category, Mouse mouse) : base(category.Trait, mouse)
        {
            Category = category;
        }

        public NominalRecord() { }
    }
}