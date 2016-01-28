using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SQLite.Net;
using SQLite.Net.Async;
using SQLite.Net.Interop;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class SqlitePlainStorage<TEntity> : IAsyncPlainStorage<TEntity> where TEntity: class, IPlainStorageEntity
    {
        private readonly SQLiteAsyncConnection asyncStorage;
        private readonly SQLiteConnectionWithLock storage;
        private readonly ILogger logger;

        public SqlitePlainStorage(ISQLitePlatform sqLitePlatform, ILogger logger,
            IAsynchronousFileSystemAccessor fileSystemAccessor, ISerializer serializer,
            SqliteSettings settings)
        {
            var entityName = typeof(TEntity).Name;
            var pathToDatabase = fileSystemAccessor.CombinePath(settings.PathToDatabaseDirectory, entityName + "-data.sqlite3");
            this.storage = new SQLiteConnectionWithLock(sqLitePlatform,
                new SQLiteConnectionString(pathToDatabase, true, new BlobSerializerDelegate(
                    serializer.SerializeToByteArray,
                    (data, type) => serializer.DeserializeFromStream(new MemoryStream(data), type),
                    (type) => true)))
            {
                TraceListener = new MvxTraceListener($"{entityName}-SQL-Queries")
            };
            this.asyncStorage = new SQLiteAsyncConnection(() => this.storage);
            this.logger = logger;
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
            try
            {
                await this.asyncStorage.RunInTransactionAsync(connection =>
                {
                    foreach (var entity in entities.Where(entity => entity != null))
                        connection.Delete(entity);
                });
                
            }
            catch (SQLiteException ex)
            {
                this.logger.Fatal($"Failed to persist {entities.Count()} entities as batch", ex);
                throw;
            }
        }

        public async Task StoreAsync(TEntity entity)
        {
            await this.StoreAsync(new[] { entity });
        }

        public virtual async Task StoreAsync(IEnumerable<TEntity> entities)
        {
            try
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
            catch (SQLiteException ex)
            {
                this.logger.Fatal($"Failed to persist {entities.Count()} entities as batch", ex);
                throw;
            }
        }

        public IReadOnlyCollection<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
            => this.RunInTransaction(table => table.Where(predicate).ToReadOnlyCollection());

        public async Task<IReadOnlyCollection<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate)
            => await this.RunInTransactionAsync(table => table.Where(predicate).ToReadOnlyCollection());

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
            => await this.RunInTransactionAsync(table => table.Count(predicate));

        public TEntity FirstOrDefault() => this.RunInTransaction(table => table.FirstOrDefault());

        public IReadOnlyCollection<TEntity> LoadAll() => this.RunInTransaction(table => table.ToReadOnlyCollection());

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
}