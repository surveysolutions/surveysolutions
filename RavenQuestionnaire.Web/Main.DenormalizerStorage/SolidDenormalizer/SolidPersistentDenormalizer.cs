// -----------------------------------------------------------------------
// <copyright file="SolidPersistentDenormalizer.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.Caching;

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
            return this.Hash.Count;
        }

        public T GetByGuid(Guid key)
        {
            if (!this.Hash.ContainsKey(key))
            {
                return null;
            }

            return this.Hash[key];
        }

        public IQueryable<T> Query()
        {
            return this.Hash.Values.AsQueryable();
        }

        public void Remove(Guid key)
        {
            T val;
            this.Hash.TryRemove(key, out val);
        }

        public void Store(T denormalizer, Guid key)
        {
            if (this.Hash.ContainsKey(key))
            {
                this.Hash[key] = denormalizer;
                return;
            }

            this.Hash.TryAdd(key, denormalizer);
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (this._memoryhash[HashKey] != null)
            {
                this._storage.Store<ConcurrentDictionary<Guid, T>>(this._memoryhash[HashKey] as ConcurrentDictionary<Guid, T>, HashKey);
            }
            this._memoryhash.Dispose();
        }

        #endregion

        protected ConcurrentDictionary<Guid, T> Hash
        {
            get
            {
                if (this._memoryhash[HashKey] == null)
                {
                    var retval = this._storage.GetByGuid<ConcurrentDictionary<Guid, T>>(HashKey);
                    if(retval==null)
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
