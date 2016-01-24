using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Genesis
{
    public class Mouse
    {
        public int Id { get; private set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public Sex Sex { get; set; }

        public int LocalityId { get; set; }

        public virtual Locality Locality { get; set; }

        public virtual ICollection<Record> Records { get; private set; }

        public Mouse() : this(null, Sex.Male, null)
        {
        }

        public Mouse(string name, Sex sex, Locality locality)
        {
            Name = name;
            Sex = sex;
            Locality = locality;
            if (locality != null)
            {
                LocalityId = locality.Id;
            }
            Records = new List<Record>();
        }

        public override string ToString()
        {
            return "Mouse " + (string.IsNullOrEmpty(Name) ? "Unknown" : Name);
        }
    }
}
