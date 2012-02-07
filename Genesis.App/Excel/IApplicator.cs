using System;
using Genesis;

namespace Genesis.Excel
{
    public interface IApplicator<TEntity>
    {
        /// <summary>
        /// Applies the value to the entity. If it is not valid it should throw an exception.
        /// </summary>
        /// <param name="entity">The entity to apply the value to.</param>
        void Apply(TEntity entity);

        bool IsKey { get; }

        bool Matches(TEntity entity);
    }
}
