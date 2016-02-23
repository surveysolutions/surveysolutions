using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement
{
    internal class TestInMemoryPlainStorageAccessor<T> : IPlainStorageAccessor<T> where T: class
    {
        private readonly Dictionary<object, T> storage = new Dictionary<object, T>();

        public IReadOnlyDictionary<object, T> Dictionary => new ReadOnlyDictionary<object, T>(this.storage);

        public T GetById(object id)
        {
            return storage[id];
        }

        public TResult Query<TResult>(Func<IQueryable<T>, TResult> query)
        {
            return query.Invoke(this.Dictionary.Values.AsQueryable());
        }

        public void Remove(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                var entityToDelete = this.storage.Where(s => s.Value == entity);

                if (entityToDelete.Any())
                    Remove(entityToDelete.First().Key);
            }
        }

        public void Remove(object id)
        {
            this.storage.Remove(id);
        }

        public void Store(IEnumerable<Tuple<T, object>> entities)
        {
            foreach (var entity in entities)
            {
                Store(entity.Item1, entity.Item2);
            }
        }

        public void Store(T entity, object id)
        {
            storage[id] = entity;
        }
    }
}