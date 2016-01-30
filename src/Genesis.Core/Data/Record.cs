using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Genesis
{
    public abstract class Record
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; protected set; }

        protected Record(Trait trait, Mouse mouse)
        {
            Trait = trait;
            Mouse = mouse;
        }

        protected Record()
        {
        }

        public int MouseId { get; set; }
        public virtual Mouse Mouse { get; set; }

        public int TraitId { get; set; }
        public virtual Trait Trait { get; set; }
    }
}