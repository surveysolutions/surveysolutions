using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Main.DenormalizerStorage
{
    public interface IQueryableDenormalizerStorage<TView> : IDenormalizerStorage<TView> where TView : class
    {
        [Obsolete]
        IQueryable<TView> Query();

        TResult Query<TResult>(Func<IQueryable<TView>, TResult> query);
    }
}
