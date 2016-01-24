using System.ComponentModel.DataAnnotations;
using System.Windows.Media;

namespace Genesis
{
    public class Species
    {
        public virtual int Id { get; private set; }
        [Required]
        public virtual string Name { get; set; }
        public virtual Color Color { get; set; }
    }
}
