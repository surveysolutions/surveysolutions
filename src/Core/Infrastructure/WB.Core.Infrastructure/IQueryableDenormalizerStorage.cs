using System;
using System.Linq;

namespace WB.Core.Infrastructure
{
    public interface IQueryableDenormalizerStorage<TView> : IDenormalizerStorage<TView>
        where TView : class, IView
    {
        [Obsolete]
        IQueryable<TView> Query();

        TResult Query<TResult>(Func<IQueryable<TView>, TResult> query);
    }
}
