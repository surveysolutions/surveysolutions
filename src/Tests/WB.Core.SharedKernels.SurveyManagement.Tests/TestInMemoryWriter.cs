using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.SharedKernels.SurveyManagement.Tests
{
    public class TestInMemoryWriter<T> : IReadSideRepositoryWriter<T> where T : class, IReadSideRepositoryEntity
    {
        private readonly Dictionary<string, T> storage = new Dictionary<string, T>();

        public IReadOnlyDictionary<string, T> Dictionary
        {
            get { return new ReadOnlyDictionary<string, T>(this.storage); }
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

        public void Remove(T view)
        {
            var keyOfItemToRemove = storage.FirstOrDefault(item => item.Value == view).Key;
            if (string.IsNullOrEmpty(keyOfItemToRemove))
                return;

            Remove(keyOfItemToRemove);
        }

        public void Store(T view, string id)
        {
            this.storage[id] = view;
        }
    }
}