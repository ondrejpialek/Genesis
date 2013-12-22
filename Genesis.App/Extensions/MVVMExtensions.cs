using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Caliburn.Micro;

namespace Genesis
{
    public static class MvvmExtensions
    {
        public static bool Set<T>(this PropertyChangedBase viewModel,
            Expression<Func<T>> propertyExpression,
            ref T field,
            T value)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                viewModel.NotifyOfPropertyChange(propertyExpression);
                return true;
            }

            return false;
        }
    }
}
