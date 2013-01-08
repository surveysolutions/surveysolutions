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

    /// <summary>
    /// The Denormalizer interface.
    /// </summary>
    public interface IDenormalizer
    {
        #region Public Methods and Operators

        /// <summary>
        /// The count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        int Count<T>() where T : class;

        /// <summary>
        /// The get by guid.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The T.
        /// </returns>
        T GetByGuid<T>(Guid key) where T : class;

        /// <summary>
        /// The query.
        /// </summary>
        /// <returns>
        /// The System.Linq.IQueryable`1[T -&gt; T].
        /// </returns>
        IQueryable<T> Query<T>() where T : class;

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        void Remove<T>(Guid key) where T : class;

        /// <summary>
        /// The store.
        /// </summary>
        /// <param name="denormalizer">
        /// The denormalizer.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        void Store<T>(T denormalizer, Guid key) where T : class;

        #endregion
    }
}