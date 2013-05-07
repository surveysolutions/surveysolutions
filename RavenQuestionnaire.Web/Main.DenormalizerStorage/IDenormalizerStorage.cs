// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDenormalizerStorage.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The DenormalizerStorage interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Main.DenormalizerStorage
{
    /// <summary>
    /// The DenormalizerStorage interface.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public interface IDenormalizerStorage<T>
        where T : class
    {
        #region Public Methods and Operators

        /// <summary>
        /// The count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        int Count();

        /// <summary>
        /// The get by guid.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The T.
        /// </returns>
        T GetByGuid(Guid key);

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        void Remove(Guid key);

        /// <summary>
        /// The store.
        /// </summary>
        /// <param name="denormalizer">
        /// The denormalizer.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        void Store(T denormalizer, Guid key);

        #endregion
    }

}