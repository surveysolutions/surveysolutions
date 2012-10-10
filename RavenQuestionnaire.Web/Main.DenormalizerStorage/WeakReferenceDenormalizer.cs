// -----------------------------------------------------------------------
// <copyright file="WeakReferenceDenormalizer.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Main.DenormalizerStorage
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class WeakReferenceDenormalizer<T> : IDenormalizerStorage<T>
        where T : class
    {
        #region Fields

        /// <summary>
        /// The _hash.
        /// </summary>
        private readonly ConcurrentDictionary<Guid, WeakReference> _hash;

        private readonly List<Guid> _bag;
        private readonly IPersistentStorage _storage;
        private readonly object _locker=new object();
        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryDenormalizer{T}"/> class.
        /// </summary>
        public WeakReferenceDenormalizer(IPersistentStorage storage)
        {
            this._hash = new ConcurrentDictionary<Guid, WeakReference>();
            this._bag = new List<Guid>();
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
            return this._bag.Count;
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
            bool lockWasTaken = false;
            var temp = _locker;
            try
            {
                Monitor.Enter(temp, ref lockWasTaken);
                {
                    if (!this._bag.Contains(key))
                    {
                        return null;
                    }
                    // Obtain an instance of a data 
                    // object from the cache of 
                    // of weak reference objects.
                    T retval;
                    if (!this._hash.ContainsKey(key) || this._hash[key].Target == null)
                    {
                        retval = this._storage.GetByGuid<T>(key);
                        if (retval == null)
                            throw new InvalidOperationException(
                                "key was present in bag but objects is missing in both caches");
                        var weekDisposable = new WeakDisposable<T>(retval, key);
                        weekDisposable.BefoureFinalize += new EventHandler(weekDisposable_BefoureFinalize);
                        var data = new WeakReference(weekDisposable, false);

                        this._hash.AddOrUpdate(key, data, (k, oldValue) => data);
                    }
                    else
                    {
                        retval = (_hash[key].Target as WeakDisposable<T>).Data as T;
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
        public IQueryable<T> Query()
        {
            throw new NotImplementedException("Query is not supproted for WeakReferenceDenormalizer");
            //    return this._hash.Values.AsQueryable();
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        public void Remove(Guid key)
        {
            bool lockWasTaken = false;
            var temp = _locker;
            try
            {
                Monitor.Enter(temp, ref lockWasTaken);
                {
                    if (!this._bag.Contains(key))
                    {
                        return;
                    }
                    this._bag.Remove(key);
                    if (this._hash.ContainsKey(key))
                    {
                        WeakReference val;
                        this._hash.TryRemove(key, out val);
                    }
                    this._storage.Remove<T>(key);
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
        /// <param name="denormalizer">
        /// The denormalizer.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        public void Store(T denormalizer, Guid key)
        {
            bool lockWasTaken = false;
            var temp = _locker;
            try
            {
                Monitor.Enter(temp, ref lockWasTaken);
                {
                    if (!this._bag.Contains(key))
                    {
                        this._bag.Add(key);
                    }

                    this._storage.Store(denormalizer, key);
                }
            }
            finally
            {
                if (lockWasTaken) Monitor.Exit(temp);
            }
        }

        #endregion


        void weekDisposable_BefoureFinalize(object sender, EventArgs e)
        {
            var data = sender as WeakDisposable<T>;

            this._storage.Store(data.Data, data.Key);
        }
    }
}
