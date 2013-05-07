namespace Main.DenormalizerStorage
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

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

        public T GetById(Guid id)
        {
            if (!this._hash.ContainsKey(id))
            {
                return null;
            }

            return this._hash[id];
        }

        public IQueryable<T> Query()
        {
            return this._hash.Values.AsQueryable();
        }

        public IEnumerable<T> Query(Expression<Func<T, bool>> predExpr)
        {
            return this._hash.Values.Where(predExpr.Compile());
        }

        public void Remove(Guid id)
        {
            T val;
            this._hash.TryRemove(id, out val);
        }

        public void Store(T view, Guid id)
        {
            if (this._hash.ContainsKey(id))
            {
                this._hash[id] = view;
                return;
            }

            this._hash.TryAdd(id, view);
        }
    }
}