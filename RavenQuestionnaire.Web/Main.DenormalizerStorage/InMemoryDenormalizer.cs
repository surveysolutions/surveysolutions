namespace Main.DenormalizerStorage
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    public class InMemoryDenormalizer<T> : IDenormalizerStorage<T>
        where T : class
    {
        private readonly ConcurrentDictionary<Guid, T> _hash;

        public InMemoryDenormalizer()
        {
            this._hash = new ConcurrentDictionary<Guid, T>();
        }

        public int Count()
        {
            return this._hash.Count;
        }

        public T GetByGuid(Guid key)
        {
            if (!this._hash.ContainsKey(key))
            {
                return null;
            }

            return this._hash[key];
        }

        public IQueryable<T> Query()
        {
            return this._hash.Values.AsQueryable();
        }

        public void Remove(Guid key)
        {
            T val;
            this._hash.TryRemove(key, out val);
        }

        public void Store(T denormalizer, Guid key)
        {
            if (this._hash.ContainsKey(key))
            {
                this._hash[key] = denormalizer;
                return;
            }

            this._hash.TryAdd(key, denormalizer);
        }
    }
}