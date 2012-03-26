using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesis;

namespace Genesis.Excel
{
    public class MouseLocalityColumn : CellReader<Mouse, string>
    {
        IEnumerable<Locality> localities;

        public MouseLocalityColumn(IEnumerable<Locality> localities)
            : base("Locality Code Lookup", true)
        {
            this.localities = localities;
        }

        protected override void Apply(Mouse entity, string value)
        {
            var locality = localities.FirstOrDefault(l => string.Equals(l.Code, value, StringComparison.InvariantCultureIgnoreCase));
            if (locality != null) {
                if (entity.Locality != null)
                {
                    entity.Locality.Mice.Remove(entity);
                }
                locality.Mice.Add(entity);
            }
            entity.Locality = locality;
        }
    }
}
