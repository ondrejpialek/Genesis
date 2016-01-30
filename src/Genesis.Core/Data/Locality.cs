using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Text;

namespace Genesis
{
    public class Locality
    {
        public int Id { get; private set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Code { get; set; }

        public DbGeography Location { get; set; }

        public virtual ICollection<Mouse> Mice { get; private set; }

        public Locality() : this(null, null) { }

        public Locality(string code, string name)
        {
            Code = code;
            Name = name;
            Mice = new List<Mouse>();
        }

        public override string ToString()
        {
            return "Locality " + (string.IsNullOrEmpty(Code) ? "Unknown" : Code);
        }
    }
}
