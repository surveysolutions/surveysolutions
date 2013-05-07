using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Main.DenormalizerStorage
{
    public interface IDenormalizerStorage<TView>
        where TView : class
    {
        int Count();

        TView GetById(Guid id);
        IEnumerable<T> Query(Expression<Func<T, bool>> predExpr);
        }

        public IEnumerable<T> Query(Expression<Func<T, bool>> predExpr)
        {
            return this._hash.Values.Where(predExpr.Compile());

        IQueryable<TView> Query();

        void Remove(Guid id);

        void Store(TView view, Guid id);
    }
}