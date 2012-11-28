// -----------------------------------------------------------------------
// <copyright file="SolidPersistentDenormalizer.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;

namespace Main.DenormalizerStorage.SolidDenormalizer
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SolidPersistentDenormalizer<T>: IDenormalizerStorage<T>, IDisposable
        where T : class
    {
        #region Fields

        private readonly IPersistentStorage _storage;
        /// <summary>
        /// The _hash.
        /// </summary>
        private readonly MemoryCache _memoryhash;
        private readonly object _locker = new object();
      /*  /// <summary>
        /// The _hash.
        /// </summary>
        private ConcurrentDictionary<Guid, T> _hash;*/
        #endregion

        #region Implementation of IDenormalizerStorage<T>

        public SolidPersistentDenormalizer(IPersistentStorage storage)
        {
            _memoryhash = new MemoryCache(typeof (T).Name);
            _storage = storage;
           // this._hash = new ConcurrentDictionary<Guid, T>();
        }

        public int Count()
        {
            int result = 0;
            ThreadSafe(() =>
                { result = this.Hash.Count; });
            return result;
        }

        public T GetByGuid(Guid key)
        {
            T result = null;
            ThreadSafe(() =>
                {
                    if (this.Hash.ContainsKey(key))
                    {
                        result = this.Hash[key];
                    }
                });
            return result;
        }

        public IQueryable<T> Query()
        {
            IQueryable<T> result = null;
            ThreadSafe(() =>
                { result= this.Hash.Values.AsQueryable(); });
            return result;
        }

        public void Remove(Guid key)
        {
            ThreadSafe(() =>
                {
                    T val;
                    this.Hash.TryRemove(key, out val);
                });
        }

        public void Store(T denormalizer, Guid key)
        {
            ThreadSafe(() =>
                {
                    if (this.Hash.ContainsKey(key))
                    {
                        this.Hash[key] = denormalizer;
                        return;
                    }

                    this.Hash.TryAdd(key, denormalizer);
                });


        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            ThreadSafe(() => { 
            if (this._memoryhash[HashKey] != null)
            {
                this._storage.Store<ConcurrentDictionary<Guid, T>>(this._memoryhash[HashKey] as ConcurrentDictionary<Guid, T>, HashKey);
            }
            this._memoryhash.Dispose();
            });
        }

        #endregion

        protected ConcurrentDictionary<Guid, T> Hash
        {
            get
            {

                if (this._memoryhash[HashKey] == null)
                {
                    var retval = this._storage.GetByGuid<ConcurrentDictionary<Guid, T>>(HashKey);
                    if (retval == null)
                    {
                        retval = new ConcurrentDictionary<Guid, T>();
                    }
                    var policy = new CacheItemPolicy();
                    policy.RemovedCallback += weekDisposable_CacheEntryRemoved;
                    policy.SlidingExpiration = TimeSpan.FromMinutes(3);
                    this._memoryhash.Add(HashKey, retval, policy);
                    return retval;
                }
                else
                {
                    return this._memoryhash[HashKey.ToString()] as ConcurrentDictionary<Guid, T>;
                }
            }

        }
        protected void ThreadSafe( Action action)
        {
            bool lockWasTaken = false;
            var temp = _locker;
            try
            {
                Monitor.Enter(temp, ref lockWasTaken);
                {
                    action();
                }
            }
            finally
            {
                if (lockWasTaken) Monitor.Exit(temp);
            }
        }

        void weekDisposable_CacheEntryRemoved(CacheEntryRemovedArguments arguments)
        {
            this._storage.Store<ConcurrentDictionary<Guid, T>>(arguments.CacheItem.Value as ConcurrentDictionary<Guid, T>, HashKey);
        }
      

        protected string HashKey
        {
            get { return typeof (T).Name; }
        }
    }
}
