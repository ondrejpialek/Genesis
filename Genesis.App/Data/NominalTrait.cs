using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Genesis
{
    public class NominalTrait : Trait
    {
        public virtual ICollection<Category> Categories { get; private set; }

        public NominalTrait()
        {
            Categories = new List<Category>();
        }
    }
}