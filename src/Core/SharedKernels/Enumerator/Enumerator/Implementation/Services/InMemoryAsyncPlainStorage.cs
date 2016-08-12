using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class InMemoryAsyncPlainStorage<TEntity> : IAsyncPlainStorage<TEntity>
        where TEntity : class, IPlainStorageEntity
    {
        public readonly Dictionary<string, TEntity> inMemroyStorage = new Dictionary<string, TEntity>();

        public TEntity GetById(string id)
        {
            return this.inMemroyStorage.ContainsKey(id) ? this.inMemroyStorage[id] : null;
        }

        public IReadOnlyCollection<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        {
            return this.inMemroyStorage.Values.AsQueryable().Where(predicate).ToReadOnlyCollection();
        }

        public TEntity FirstOrDefault()
        {
            return this.inMemroyStorage.Values.AsQueryable().FirstOrDefault();
        }

        public IReadOnlyCollection<TEntity> LoadAll()
        {
            return this.inMemroyStorage.Values.ToReadOnlyCollection();
        }

        public int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return this.inMemroyStorage.Values.AsQueryable().Count(predicate);
        }

        public async Task RemoveAllAsync() => await Task.Run(() => this.inMemroyStorage.Clear());

        public async Task<TEntity> GetByIdAsync(string id) => await Task.Run(() => this.GetById(id));

        public async Task StoreAsync(TEntity entity) => await Task.Run(() => this.inMemroyStorage[entity.Id] = entity);

        public async Task<IReadOnlyCollection<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate) => await Task.Run(() => this.Where(predicate));

        public IReadOnlyCollection<TEntity> FixedQuery(Expression<Func<TEntity, bool>> wherePredicate, Expression<Func<TEntity, int>> orderPredicate, int takeCount)
        {
            return this.inMemroyStorage.Values.AsQueryable().Where(wherePredicate).OrderBy(orderPredicate).Take(takeCount).ToReadOnlyCollection();
        }

        public async Task<IReadOnlyCollection<TEntity>> LoadAllAsync() => await Task.Run(this.LoadAllAsync);

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate) => await Task.Run(() => this.CountAsync(predicate));

        public async Task RemoveAsync(string id)
        {
            await Task.Run(() =>
            {
                if (this.inMemroyStorage.ContainsKey(id))
                    this.inMemroyStorage.Remove(id);
            });
        }

        public void Remove(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities.Where(entity => entity != null))
            {
                if (this.inMemroyStorage.ContainsKey(entity.Id))
                    this.inMemroyStorage.Remove(entity.Id);
            }
        }

        public async Task RemoveAsync(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities.Where(entity => entity != null))
            {
                await this.RemoveAsync(entity.Id);
            }
        }

        public async Task StoreAsync(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities.Where(entity => entity != null))
            {
                await this.StoreAsync(entity);
            }
        }

        public void Dispose()
        {
            this.inMemroyStorage.Clear();
        }
    }
}