using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement
{
    internal class TestAsyncPlainStorage<T> : IAsyncPlainStorage<T> where T : class, IPlainStorageEntity
    {
        private readonly List<T> items;

        public TestAsyncPlainStorage(IEnumerable<T> items)
        {
            this.items = new List<T>(items);
        } 

        public T GetById(string id) => this.items.FirstOrDefault(x => x.Id == id);

        public async Task RemoveAsync(string id)
            => await Task.Run(()
                => this.items.Remove(this.GetById(id)));

        public async Task RemoveAsync(IEnumerable<T> entities)
            => await Task.Run(()
                => entities.ToList().ForEach(entity => this.items.Remove(entity)));

        public async Task StoreAsync(T entity)
            => await Task.Run(()
                => this.items.Add(entity));

        public async Task StoreAsync(IEnumerable<T> entities)
            => await Task.Run(()
                => this.items.AddRange(entities));

        public T FirstOrDefault()
            => this.items.FirstOrDefault();

        public IReadOnlyCollection<T> LoadAll()
            => this.items.ToReadOnlyCollection();

        public int Count(Expression<Func<T, bool>> predicate)
            => this.items.Count(predicate.Compile());

        public IReadOnlyCollection<T> Where(Expression<Func<T, bool>> predicate)
            => this.items.Where(predicate.Compile()).ToReadOnlyCollection();

        public void Dispose() {}
    }
}