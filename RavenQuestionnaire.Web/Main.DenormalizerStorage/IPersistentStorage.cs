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
    public interface IPersistentStorage
        
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
        T GetByGuid<T>(Guid key) where T : class;
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
    }
}
