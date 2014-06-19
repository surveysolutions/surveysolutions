using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public void Store(T view, string id)
        {
            this.storage[id] = view;
        }
    }
}