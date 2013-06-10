// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDenormalizer.cs" company="">
//   
// </copyright>
// <summary>
//   The Denormalizer interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.Infrastructure;

namespace Main.DenormalizerStorage
{
    using System;
    using System.Linq;

    public interface IDenormalizer
    {
        int Count<T>()
            where T : class, IView;

        T GetByGuid<T>(Guid key)
            where T : class, IView;

        [Obsolete]
        IQueryable<T> Query<T>()
            where T : class, IView;

        TResult Query<T, TResult>(Func<IQueryable<T>, TResult> query)
            where T : class, IView;

        void Remove<T>(Guid key)
            where T : class, IView;

        void Store<T>(T denormalizer, Guid key)
            where T : class, IView;
    }
}