using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genesis;

namespace Genesis.Excel
{
    public class Applicator<TEntity, TValue> : IApplicator<TEntity>
    {
        protected TValue value;

        protected Action<TEntity, TValue> application;

        protected Func<TEntity, TValue, bool> comparator;

        public bool IsKey
        {
            get;
            protected set;
        }

        protected Applicator(Action<TEntity, TValue> application, TValue value, Func<TEntity, TValue, bool> comparator, bool isKey)
        {
            this.application = application;
            this.value = value;
            this.comparator = comparator;
            this.IsKey = isKey;
        }

        public Applicator(Action<TEntity, TValue> application, TValue value) : this(application, value, null, false) { }

        public Applicator(Action<TEntity, TValue> application, TValue value, Func<TEntity, TValue, bool> comparator) : this(application, value, comparator, true) { }

        public void Apply(TEntity entity)
        {
            this.application.Invoke(entity, value);
        }

        public bool Matches(TEntity entity)
        {
            if (comparator == null)
                return true;

            return comparator(entity, value);
        }
    }

}
