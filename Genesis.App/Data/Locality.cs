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
        public virtual int Id { get; private set; }
        [Required]
        public virtual String Name { get; set; }
        [Required]
        public virtual String Code { get; set; }
        public virtual DbGeography Location { get; set; }

        public virtual ICollection<Mouse> Mice { get; private set; }

        public Locality()
        {
            Mice = new List<Mouse>();
        }

        public override string ToString()
        {
            return "Locality " + (string.IsNullOrEmpty(Code) ? "Unknown" : Code);
        }
    }
}
