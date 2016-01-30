using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesis;

namespace Genesis.Excel
{
    public class RowApplicator<TEntity>
    {
        protected IEnumerable<IApplicator<TEntity>> applicators;

        public RowApplicator(IEnumerable<IApplicator<TEntity>> applicators)
        {
            this.applicators = new List<IApplicator<TEntity>>(applicators);
        }

        public void Apply(TEntity entity)
        {
            foreach (var applicator in applicators)
            {
                applicator.Apply(entity);
            }
        }

        public bool Matches(TEntity entity)
        {
            return applicators.Where(a => a.IsKey).Select(a => a.Matches(entity)).Aggregate((a, b) => a && b);
        }
    }
}
