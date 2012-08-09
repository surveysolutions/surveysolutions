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
    }

    public class InMemoryDenormalizer<T> : IDenormalizerStorage<T> where T : class
    {
        private ConcurrentDictionary<Guid, T> hash;

        public InMemoryDenormalizer()
        {
            this.hash=new ConcurrentDictionary<Guid, T>();
        }

        #region Implementation of IDenormalizerStorage

        public T GetByGuid(Guid key) 
        {
            if (!this.hash.ContainsKey(key))
                return null;
            return this.hash[key];
            
        }

        public IQueryable<T> Query()
        {
            return this.hash.Values.AsQueryable();
            
        }

        public void Store(T denormalizer, Guid key)
        {
            if (this.hash.ContainsKey(key))
            {

                hash[key] = denormalizer;
                return;
            }
            hash.TryAdd(key, denormalizer);
            
        }


        public void Remove(Guid key)
        {
            T val;
            hash.TryRemove(key, out val);
        }

        #endregion
    }
}
