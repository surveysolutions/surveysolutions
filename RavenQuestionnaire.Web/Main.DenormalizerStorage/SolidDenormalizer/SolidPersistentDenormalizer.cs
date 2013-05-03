// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SolidPersistentDenormalizer.cs" company="">
//   
// </copyright>
// <summary>
//   The solid persistent denormalizer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.DenormalizerStorage.SolidDenormalizer
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Threading;

    /// <summary>
    /// The solid persistent denormalizer.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class SolidPersistentDenormalizer<T> : IDenormalizerStorage<T>, IDisposable
        where T : class
    {
        #region Fields

        /// <summary>
        /// The _locker.
        /// </summary>
        private readonly object _locker = new object();

        /// <summary>
        /// The _hash.
        /// </summary>
        private readonly MemoryCache _memoryhash;

        /// <summary>
        /// The _storage.
        /// </summary>
        private readonly IPersistentStorage _storage;

        #endregion

        /*  /// <summary>
        /// The _hash.
        /// </summary>
        private ConcurrentDictionary<Guid, T> _hash;*/
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SolidPersistentDenormalizer{T}"/> class.
        /// </summary>
        /// <param name="storage">
        /// The storage.
        /// </param>
        public SolidPersistentDenormalizer(IPersistentStorage storage)
        {
            this._memoryhash = new MemoryCache(typeof(T).Name);
            this._storage = storage;

            // this._hash = new ConcurrentDictionary<Guid, T>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the hash.
        /// </summary>
        protected ConcurrentDictionary<Guid, T> Hash
        {
            get
            {
                if (this._memoryhash[this.HashKey] == null)
                {
                    var retval = this._storage.GetByGuid<ConcurrentDictionary<Guid, T>>(this.HashKey);
                    if (retval == null)
                    {
                        retval = new ConcurrentDictionary<Guid, T>();
                    }

                    var policy = new CacheItemPolicy();
                    policy.RemovedCallback += this.weekDisposable_CacheEntryRemoved;
                    policy.SlidingExpiration = TimeSpan.FromMinutes(3);
                    this._memoryhash.Add(this.HashKey, retval, policy);
                    return retval;
                }
                else
                {
                    return this._memoryhash[this.HashKey] as ConcurrentDictionary<Guid, T>;
                }
            }
        }

        /// <summary>
        /// Gets the hash key.
        /// </summary>
        protected string HashKey
        {
            get
            {
                return typeof(T).Name;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The count.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int Count()
        {
            int result = 0;
            this.ThreadSafe(() => { result = this.Hash.Count; });
            return result;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.ThreadSafe(
                () =>
                    {
                        if (this._memoryhash[this.HashKey] != null)
                        {
                            this._storage.Store(
                                this._memoryhash[this.HashKey] as ConcurrentDictionary<Guid, T>, this.HashKey);
                        }

                        this._memoryhash.Dispose();
                    });
        }

        /// <summary>
        /// The get by guid.
        /// </summary>
        /// <param name="id">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public T GetById(Guid id)
        {
            T result = null;
            this.ThreadSafe(
                () =>
                    {
                        if (this.Hash.ContainsKey(id))
                        {
                            result = this.Hash[id];
                        }
                    });
            return result;
        }

        /// <summary>
        /// The query.
        /// </summary>
        /// <returns>
        /// The <see cref="IQueryable"/>.
        /// </returns>
        public IQueryable<T> Query()
        {
            IQueryable<T> result = null;
            this.ThreadSafe(() => { result = this.Hash.Values.AsQueryable(); });
            return result;
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="id">
        /// The key.
        /// </param>
        public void Remove(Guid id)
        {
            this.ThreadSafe(
                () =>
                    {
                        T val;
                        this.Hash.TryRemove(id, out val);
                    });
        }

        /// <summary>
        /// The store.
        /// </summary>
        /// <param name="view">
        /// The denormalizer.
        /// </param>
        /// <param name="id">
        /// The key.
        /// </param>
        public void Store(T view, Guid id)
        {
            this.ThreadSafe(
                () =>
                    {
                        if (this.Hash.ContainsKey(id))
                        {
                            this.Hash[id] = view;
                            return;
                        }

                        this.Hash.TryAdd(id, view);
                    });
        }

        #endregion

        #region Methods

        /// <summary>
        /// The thread safe.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        protected void ThreadSafe(Action action)
        {
            bool lockWasTaken = false;
            object temp = this._locker;
            try
            {
                Monitor.Enter(temp, ref lockWasTaken);
                {
                    action();
                }
            }
            finally
            {
                if (lockWasTaken)
                {
                    Monitor.Exit(temp);
                }
            }
        }

        /// <summary>
        /// The week disposable_ cache entry removed.
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        private void weekDisposable_CacheEntryRemoved(CacheEntryRemovedArguments arguments)
        {
            this._storage.Store(arguments.CacheItem.Value as ConcurrentDictionary<Guid, T>, this.HashKey);
        }

        #endregion
    }
}