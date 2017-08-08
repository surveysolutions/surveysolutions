using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Abc.Storage
{
    public class TestPlainStorage<T> : IPlainStorageAccessor<T> where T : class
    {
        private readonly Dictionary<object, T> entites;

        public TestPlainStorage()
        {

        }
        public TestPlainStorage(Dictionary<object, T> entites = null)
        {
            this.entites = entites ?? new Dictionary<object, T>();
        }

        public T GetById(object id)
        {
            if (!this.entites.ContainsKey(id))
                return null;
            return this.entites[id];
        }

        public void Remove(object id)
        {
            this.entites.Remove(id);
        }

        public void Remove(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                var itemToRemove = this.entites.SingleOrDefault(x => x.Value.Equals(entity));
                this.entites.Remove(itemToRemove.Key);
            }
        }

        public virtual void Store(T entity, object id)
        {
            if (id != null && this.entites.ContainsKey(id))
            {
                this.entites[id] = entity;
            }
            else
            {
                this.entites.Add(id ?? this.entites.Count + 1, entity);
            }
        }

        public void Store(IEnumerable<Tuple<T, object>> entitiesToStore)
        {
            foreach (var keyValuePair in entitiesToStore)
            {
                this.Store(keyValuePair.Item1, keyValuePair.Item2);
            }
        }

        public TResult Query<TResult>(Func<IQueryable<T>, TResult> query)
        {
            return query.Invoke(this.entites.Values.AsQueryable());
        }

        public IEnumerable<T> Query(Func<T, bool> query)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Query(Expression<T> query)
        {
            throw new NotImplementedException();
        }

        public void Clear() { this.entites.Clear();}
    }
}