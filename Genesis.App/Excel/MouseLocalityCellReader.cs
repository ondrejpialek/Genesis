using System;
using System.Collections.Generic;
using System.Linq;

namespace Genesis.Excel
{
    public class MouseLocalityCellReader : CellReader<Mouse, string>
    {
        Dictionary<string, Locality> localities;

        public MouseLocalityCellReader(IEnumerable<Locality> localities)
            : base("Locality Code Lookup", true)
        {
            this.localities = localities.ToDictionary(l => l.Code.Trim().ToLowerInvariant(), l => l);
        }

        protected override void Apply(Mouse entity, string value)
        {
            var code = value.Trim().ToLowerInvariant();

            Locality locality;
            if (!localities.TryGetValue(code, out locality))
                throw new Exception("Cannot find locality " + value);

            locality.Mice.Add(entity);
            entity.Locality = locality;
            entity.LocalityId = locality.Id;
        }
    }
}
