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
        /// The query.
        /// </summary>
        /// <returns>
        /// The System.Linq.IQueryable`1[T -&gt; T].
        /// </returns>
        IQueryable<T> Query();

        IEnumerable<T> Query(Expression<Func<T, bool>> predExpr);
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

    /// <summary>
    /// The in memory denormalizer.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class InMemoryDenormalizer<T> : IDenormalizerStorage<T>
        where T : class
    {
        #region Fields

        /// <summary>
        /// The _hash.
        /// </summary>
        private readonly ConcurrentDictionary<Guid, T> _hash;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryDenormalizer{T}"/> class.
        /// </summary>
        public InMemoryDenormalizer()
        {
            this._hash = new ConcurrentDictionary<Guid, T>();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int Count()
        {
            return this._hash.Count;
        }

        /// <summary>
        /// The get by guid.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The T.
        /// </returns>
        public T GetByGuid(Guid key)
        {
            if (!this._hash.ContainsKey(key))
            {
                return null;
            }

            return this._hash[key];
        }

        /// <summary>
        /// The query.
        /// </summary>
        /// <returns>
        /// The System.Linq.IQueryable`1[T -&gt; T].
        /// </returns>
        public IQueryable<T> Query()
        {
            return this._hash.Values.AsQueryable();
        }

        public IEnumerable<T> Query(Expression<Func<T, bool>> predExpr)
        {
            return this._hash.Values.Where(predExpr.Compile());
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        public void Remove(Guid key)
        {
            T val;
            this._hash.TryRemove(key, out val);
        }

        /// <summary>
        /// The store.
        /// </summary>
        /// <param name="denormalizer">
        /// The denormalizer.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        public void Store(T denormalizer, Guid key)
        {
            if (this._hash.ContainsKey(key))
            {
                this._hash[key] = denormalizer;
                return;
            }

            this._hash.TryAdd(key, denormalizer);
        }

        #endregion
    }
}