using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace WB.Core.Infrastructure
{
    public interface IFilterableDenormalizerStorage<TView> : IDenormalizerStorage<TView>
        where TView : class
    {
        IEnumerable<TView> Query(Expression<Func<TView, bool>> predicate);
    }
}
