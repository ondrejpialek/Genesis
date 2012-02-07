using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Genesis
{
    public class Species
    {
        public virtual int Id { get; private set; }
        public virtual string Name { get; set; }
        public virtual Color Color { get; set; }
    }
}
