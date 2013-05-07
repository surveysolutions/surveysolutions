using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Main.DenormalizerStorage
{
    public interface IFilterableDenormalizerStorage<T> : IDenormalizerStorage<T> where T : class
    {
        IEnumerable<T> Query(Expression<Func<T, bool>> predExpr);
    }
}
