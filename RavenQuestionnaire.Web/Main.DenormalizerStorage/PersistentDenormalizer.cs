// -----------------------------------------------------------------------
// <copyright file="WeakReferenceDenormalizer.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;
using System.Threading;

namespace Main.DenormalizerStorage
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class PersistentDenormalizer<TView> : IDenormalizerStorage<TView>, IDisposable
        where TView : class
    {
        #region Fields

        /// <summary>
        /// The _hash.
        /// </summary>
        private readonly MemoryCache _hash;

      //  private readonly List<Guid> _bag;
        private readonly IPersistentStorage _storage;
        private readonly object _locker=new object();
        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryDenormalizer{T}"/> class.
        /// </summary>
        public PersistentDenormalizer(IPersistentStorage storage)
            : this(new MemoryCache("PersistentDenormalizer"), storage/*, new List<Guid>()*/)
        {
        }
        public PersistentDenormalizer(MemoryCache hash, IPersistentStorage storage/*, List<Guid> bag*/)
        {
            this._hash = hash;
          //  this._bag = bag;
            this._storage = storage;
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
           // return this._bag.Count;
            throw new NotImplementedException("Query is not supproted for WeakReferenceDenormalizer");
        }

        /// <summary>
        /// The get by guid.
        /// </summary>
        /// <param name="id">
        /// The key.
        /// </param>
        /// <returns>
        /// The T.
        /// </returns>
        public TView GetById(Guid id)
        {
            bool lockWasTaken = false;
            var temp = _locker;
            try
            {
                Monitor.Enter(temp, ref lockWasTaken);
                {
                    // Obtain an instance of a data 
                    // object from the cache of 
                    // of weak reference objects.
                    TView retval;
                    if (!this._hash.Contains(id.ToString()))
                    {
                        retval = this._storage.GetByGuid<TView>(id.ToString());
                        if (retval == null)
                            throw new InvalidOperationException(
                                "key was present in bag but objects is missing in both caches");
                      /*  if (!this._bag.Contains(key))
                        {
                            this._bag.Add(key);
                        }*/
                        var policy = new CacheItemPolicy();
                        policy.RemovedCallback += weekDisposable_CacheEntryRemoved;
                        policy.SlidingExpiration = TimeSpan.FromMinutes(3);
                        this._hash.Add(id.ToString(), retval, policy);
                    }
                    else
                    {
                      //  retval = (_hash[key].Target as WeakDisposable<T>).Data as T;
                        retval = _hash[id.ToString()] as TView;
                    }
                    return retval;
                }
            }
            finally
            {
                if (lockWasTaken) Monitor.Exit(temp);
            }
        }

        /// <summary>
        /// The query.
        /// </summary>
        /// <returns>
        /// The System.Linq.IQueryable`1[T -&gt; T].
        /// </returns>
        public IQueryable<TView> Query()
        {
            throw new NotImplementedException("Query is not supproted for WeakReferenceDenormalizer");
            //    return this._hash.Values.AsQueryable();
        }

        public IEnumerable<TView> Query(Expression<Func<TView, bool>> predExpr)
        {
            throw new NotImplementedException("Query is not supproted for WeakReferenceDenormalizer");
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="id">
        /// The key.
        /// </param>
        public void Remove(Guid id)
        {
            bool lockWasTaken = false;
            var temp = _locker;
            try
            {
                Monitor.Enter(temp, ref lockWasTaken);
                {
                   /* if (!this._bag.Contains(key))
                    {
                        return;
                    }
                    this._bag.Remove(key);*/
                    if (this._hash.Contains(id.ToString()))
                    {
                        this._hash.Remove(id.ToString());
                    }
                    this._storage.Remove<TView>(id.ToString());
                }
            }
            finally
            {
                if (lockWasTaken) Monitor.Exit(temp);
            }
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
        public void Store(TView view, Guid id)
        {
            bool lockWasTaken = false;
            var temp = _locker;
            try
            {
                Monitor.Enter(temp, ref lockWasTaken);
                {
                   /* if (!this._bag.Contains(key))
                    {
                        this._bag.Add(key);
                    }*/

                   // this._storage.Store<T>(denormalizer, key);

                    if (this._hash[id.ToString()]==null)
                    {
                        var policy = new CacheItemPolicy();
                        policy.RemovedCallback += weekDisposable_CacheEntryRemoved;
                        policy.SlidingExpiration = TimeSpan.FromSeconds(10);
                        this._hash.Add(id.ToString(), view, policy);
                    }else
                    {
                        this._hash[id.ToString()] = view;
                    }
                }
            }
            finally
            {
                if (lockWasTaken) Monitor.Exit(temp);
            }
        }

        #endregion


        void weekDisposable_CacheEntryRemoved(CacheEntryRemovedArguments arguments)
        {
            Guid key = Guid.Parse(arguments.CacheItem.Key);
          //  if (this._bag.Contains(key))
                this._storage.Store<TView>(arguments.CacheItem.Value as TView, key.ToString());
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            this._hash.Dispose();
        }

        #endregion
    }
}
