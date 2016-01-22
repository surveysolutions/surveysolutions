using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public T GetById(string id)
        {
            return this.items.FirstOrDefault(x => x.Id == id);
        }

        public async Task RemoveAsync(string id)
        {
            await Task.Run(() =>
            {
                this.items.Remove(this.GetById(id));
            });
        }

        public async Task RemoveAsync(IEnumerable<T> entities)
        {
            await Task.Run(() =>
            {
                entities.ToList().ForEach(entity => this.items.Remove(entity));
            });
        }

        public async Task StoreAsync(T entity)
        {
            await Task.Run(() =>
            {
                this.items.Add(entity);
            });
        }

        public async Task StoreAsync(IEnumerable<T> entities)
        {
            await Task.Run(() =>
            {
                this.items.AddRange(entities);
            });
        }

        public TResult Query<TResult>(Func<IQueryable<T>, TResult> query)
        {
            return query.Invoke(this.items.AsQueryable());
        }

        public void Dispose()
        {
        }
    }
}