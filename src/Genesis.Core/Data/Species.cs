using System.ComponentModel.DataAnnotations;

namespace Genesis
{
    public class Species
    {
        public virtual int Id { get; private set; }
        [Required]
        public virtual string Name { get; set; }
    }
}
