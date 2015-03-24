using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Unit
{
    public class TestPlainStorage<T> : IQueryablePlainStorageAccessor<T> where T : class
    {
        private readonly Dictionary<object, T> entites;

        public TestPlainStorage()
        {
            this.entites = new Dictionary<object, T>();
        }

        public T GetById(object id)
        {
            return this.entites[id];
        }

        public void Remove(object id)
        {
            this.entites.Remove(id);
        }

        public void Store(T entity, object id)
        {
            if (this.entites.ContainsKey(id))
            {
                this.entites[id] = entity;
            }
            else
            {
                this.entites.Add(id, entity);
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
    }
}