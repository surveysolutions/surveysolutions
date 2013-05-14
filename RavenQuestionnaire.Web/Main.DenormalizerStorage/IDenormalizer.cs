// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDenormalizer.cs" company="">
//   
// </copyright>
// <summary>
//   The Denormalizer interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.DenormalizerStorage
{
    using System;
    using System.Linq;

    public interface IDenormalizer
    {
        int Count<T>() where T : class;

        T GetByGuid<T>(Guid key) where T : class;

        [Obsolete]
        IQueryable<T> Query<T>() where T : class;

        TResult Query<T, TResult>(Func<IQueryable<T>, TResult> query) where T : class;

        void Remove<T>(Guid key) where T : class;

        void Store<T>(T denormalizer, Guid key) where T : class;
    }
}