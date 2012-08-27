using System;
using System.Collections.Concurrent;
using System.Linq;

namespace RavenQuestionnaire.Core.Denormalizers
{
    public interface IDenormalizerStorage<T> where T : class
    {
        T GetByGuid(Guid key);
        IQueryable<T> Query();
        void Store(T denormalizer, Guid key);
        void Remove(Guid key);
        int Count();
    }

    public class InMemoryDenormalizer<T> : IDenormalizerStorage<T> where T : class
    {
        private readonly ConcurrentDictionary<Guid, T> _hash;

        public InMemoryDenormalizer()
        {
            this._hash = new ConcurrentDictionary<Guid, T>();
        }

        #region Implementation of IDenormalizerStorage

        public T GetByGuid(Guid key) 
        {
            if (!this._hash.ContainsKey(key))
                return null;
            return this._hash[key];
            
        }

        public IQueryable<T> Query()
        {
            return this._hash.Values.AsQueryable();
            
        }

        public void Store(T denormalizer, Guid key)
        {
            if (this._hash.ContainsKey(key))
            {

                _hash[key] = denormalizer;
                return;
            }
            _hash.TryAdd(key, denormalizer);
        }

        public void Remove(Guid key)
        {
            T val;
            _hash.TryRemove(key, out val);
        }

        public int Count()
        {
            return _hash.Count;
        }

        #endregion
    }
}
