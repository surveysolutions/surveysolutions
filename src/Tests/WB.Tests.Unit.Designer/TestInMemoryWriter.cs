using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.Designer
{
    internal class TestInMemoryWriter<T> : IReadSideRepositoryWriter<T>,
        IReadSideKeyValueStorage<T>,
        IQueryableReadSideRepositoryReader<T>
        where T : class, IReadSideRepositoryEntity
    {
        private readonly Dictionary<string, T> storage = new Dictionary<string, T>();

        public IReadOnlyDictionary<string, T> Dictionary
        {
            get { return new ReadOnlyDictionary<string, T>(this.storage); }
        }

        public int Count()
        {
            return this.Dictionary.Count;
        }

        public T GetById(string id)
        {
            T result;
            this.storage.TryGetValue(id, out result);
            return result;
        }

        public void Remove(string id)
        {
            this.storage.Remove(id);
        }

        public void RemoveIfStartsWith(string beginingOfId)
        {
            var allKeyToRemove = this.storage.Keys.Where(k => k.StartsWith(beginingOfId)).ToArray();
            foreach (var keyToRemove in allKeyToRemove)
            {
                this.storage.Remove(keyToRemove);
            }
        }

        public IEnumerable<string> GetIdsStartWith(string beginingOfId)
        {
            return this.storage.Keys.Where(k => k.StartsWith(beginingOfId)).ToList();
        }

        public void Remove(T view)
        {
            var keyOfItemToRemove = this.storage.FirstOrDefault(item => item.Value == view).Key;
            if (string.IsNullOrEmpty(keyOfItemToRemove))
                return;

            this.Remove(keyOfItemToRemove);
        }

        public void Store(T view, string id)
        {
            this.storage[id] = view;
        }

        public void BulkStore(List<Tuple<T, string>> bulk)
        {
            foreach (var tuple in bulk)
            {
                this.Store(tuple.Item1, tuple.Item2);
            }
        }

        public Type ViewType
        {
            get { return typeof(T); }
        }

        public string GetReadableStatus()
        {
            return "Test";
        }

        public TResult Query<TResult>(Func<IQueryable<T>, TResult> query)
        {
            return query.Invoke(this.Dictionary.Values.AsQueryable());
        }
    }
}