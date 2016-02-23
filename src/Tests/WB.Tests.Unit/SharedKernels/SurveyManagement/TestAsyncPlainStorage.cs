using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SQLite.Net;
using SQLite.Net.Async;
using SQLite.Net.Platform.Win32;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement
{
    internal class SqliteInmemoryStorage<TEntity> : IAsyncPlainStorage<TEntity> where TEntity : class , IPlainStorageEntity
    {
        private readonly SQLiteConnectionWithLock storage;
        private readonly SQLiteAsyncConnection asyncStorage;

        public SqliteInmemoryStorage()
        {
            var serializer = new NewtonJsonSerializer(new JsonSerializerSettingsFactory()); // TODO find a way to use portable newtonsoft
            this.storage = new SQLiteConnectionWithLock(new SQLitePlatformWin32(),
                new SQLiteConnectionString(":memory:", true, new BlobSerializerDelegate(
                    serializer.SerializeToByteArray,
                    (data, type) => serializer.DeserializeFromStream(new MemoryStream(data), type),
                    (type) => true)));

            this.asyncStorage = new SQLiteAsyncConnection(() => this.storage);
            this.storage.CreateTable<TEntity>();
            this.storage.CreateIndex<TEntity>(entity => entity.Id);
        }

        public virtual TEntity GetById(string id)
        {
            TEntity entity = null;
            this.storage.RunInTransaction(() => entity = this.storage.Find<TEntity>(x => x.Id == id));
            return entity;
        }

        public async Task RemoveAsync(string id)
        {
            TEntity entity = this.GetById(id);

            await this.RemoveAsync(new[] { entity });
        }

        public virtual async Task RemoveAsync(IEnumerable<TEntity> entities)
        {
            await this.asyncStorage.RunInTransactionAsync(connection =>
            {
                foreach (var entity in entities.Where(entity => entity != null))
                    connection.Delete(entity);
            });
        }

        public async Task StoreAsync(TEntity entity)
        {
            await this.StoreAsync(new[] { entity }).ConfigureAwait(false);
        }

        public virtual async Task StoreAsync(IEnumerable<TEntity> entities)
        {
            await this.asyncStorage.RunInTransactionAsync(connection =>
            {
                foreach (var entity in entities.Where(entity => entity != null))
                {
                    var isEntityExists = connection.Table<TEntity>().Count(x => x.Id == entity.Id) > 0;

                    if (isEntityExists)
                    {
                        connection.Update(entity);
                    }
                    else
                    {
                        connection.Insert(entity);
                    }
                }
            }).ConfigureAwait(false);
        }

        public IReadOnlyCollection<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
            => this.RunInTransaction(table => table.Where(predicate).ToReadOnlyCollection());

        public async Task<IReadOnlyCollection<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate)
            => await this.RunInTransactionAsync(table => table.Where(predicate).ToReadOnlyCollection());

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
            => await this.RunInTransactionAsync(table => table.Count(predicate));

        public TEntity FirstOrDefault() => this.RunInTransaction(table => table.FirstOrDefault());

        public IReadOnlyCollection<TEntity> LoadAll() => this.RunInTransaction(table => table.ToReadOnlyCollection());

        public async Task<IReadOnlyCollection<TEntity>> LoadAllAsync()
            => await this.RunInTransactionAsync(table => table.ToReadOnlyCollection());

        private TResult RunInTransaction<TResult>(Func<TableQuery<TEntity>, TResult> function)
        {
            TResult result = default(TResult);
            this.storage.RunInTransaction(() => result = function.Invoke(this.storage.Table<TEntity>()));
            return result;
        }

        private async Task<TResult> RunInTransactionAsync<TResult>(Func<TableQuery<TEntity>, TResult> function)
        {
            TResult result = default(TResult);
            await this.asyncStorage.RunInTransactionAsync(connection => result = function.Invoke(connection.Table<TEntity>()));
            return result;
        }

        public void Dispose()
        {
            this.storage.Dispose();
        }
    }

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

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
            => await Task.FromResult(this.items.Count(predicate.Compile()));

        public T FirstOrDefault()
            => this.items.FirstOrDefault();

        public IReadOnlyCollection<T> LoadAll()
            => this.items.ToReadOnlyCollection();

        public async Task<IReadOnlyCollection<T>> LoadAllAsync()
            => await Task.FromResult(this.items.ToReadOnlyCollection());

        public IReadOnlyCollection<T> Where(Expression<Func<T, bool>> predicate)
            => this.items.Where(predicate.Compile()).ToReadOnlyCollection();

        public async Task<IReadOnlyCollection<T>> WhereAsync(Expression<Func<T, bool>> predicate)
            => await Task.FromResult(this.items.Where<T>(predicate.Compile()).ToReadOnlyCollection());

        public void Dispose() {}
    }
}