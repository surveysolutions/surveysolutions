// -----------------------------------------------------------------------
// <copyright file="IPersistentStorage.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Main.DenormalizerStorage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IPersistentStorage<T>
        where T : class
    {
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
    }
}
