using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using WB.Core.Infrastructure;

namespace Main.DenormalizerStorage
{
    public class InMemoryDenormalizer<TView> : IQueryableDenormalizerStorage<TView>
        where TView : class, IView
    {
        private readonly ConcurrentDictionary<Guid, TView> _hash;

        public InMemoryDenormalizer()
        {
            this._hash = new ConcurrentDictionary<Guid, TView>();
        }

        public int Count()
        {
            return this._hash.Count;
        }

        public TView GetById(Guid id)
        {
            if (!this._hash.ContainsKey(id))
            {
                return null;
            }

            return this._hash[id];
        }

        public IQueryable<TView> Query()
        {
            return this._hash.Values.AsQueryable();
        }

        public TResult Query<TResult>(Func<IQueryable<TView>, TResult> query)
        {
            return query.Invoke(this.Query());
        }

        public void Remove(Guid id)
        {
            TView val;
            this._hash.TryRemove(id, out val);
        }

        public void Store(TView view, Guid id)
        {
            if (this._hash.ContainsKey(id))
            {
                this._hash[id] = view;
                return;
            }

            this._hash.TryAdd(id, view);
        }

        public void Clear()
        {
            this._hash.Clear();
        }
    }
}