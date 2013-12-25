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
            if (entity.Locality != null)
            {
                if (entity.Locality.Code.Equals(value,StringComparison.InvariantCultureIgnoreCase))
                    return;

                entity.Locality.Mice.Remove(entity);
            }

            var locality = localities.FirstOrDefault(l => string.Equals(l.Code, value, StringComparison.InvariantCultureIgnoreCase));
            if (locality == null)
                throw new Exception("Cannot find locality " + value);
            locality.Mice.Add(entity);
            entity.Locality = locality;
        }
    }
}
