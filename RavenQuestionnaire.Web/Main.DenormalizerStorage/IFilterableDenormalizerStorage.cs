using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Main.DenormalizerStorage
{
    public interface IFilterableDenormalizerStorage<TView> : IDenormalizerStorage<TView> where TView : class
    {
        IEnumerable<TView> Query(Expression<Func<TView, bool>> predicate);
    }
}
