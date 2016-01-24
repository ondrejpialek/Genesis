using System.ComponentModel.DataAnnotations;

namespace Genesis
{
    public abstract class Trait
    {
        public virtual int Id { get; private set; }

        [Required]
        public virtual string Name { get; set; }
    }
}