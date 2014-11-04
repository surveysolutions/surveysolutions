using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Raven.Client.Linq;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Supervisor.Tests
{
    public class TestPlainStorage<T> : IQueryablePlainStorageAccessor<T> where T : class
    {
        private readonly Dictionary<string, T> entites;

        public TestPlainStorage()
        {
            entites = new Dictionary<string, T>();
        }

        public T GetById(string id)
        {
            return entites[id];
        }

        public void Remove(string id)
        {
            entites.Remove(id);
        }

        public void Store(T entity, string id)
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

        public void Store(IEnumerable<Tuple<T, string>> entitiesToStore)
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