using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using SQLite.Net;
using SQLite.Net.Async;
using SQLite.Net.Interop;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class SqlitePlainStorage<TEntity> : IAsyncPlainStorage<TEntity>
        where TEntity : class, IPlainStorageEntity
    {
        protected readonly SQLiteAsyncConnection asyncStorage;
        protected readonly SQLiteConnectionWithLock connection;
        private readonly ILogger logger;

        public SqlitePlainStorage(ISQLitePlatform sqLitePlatform,
            ILogger logger,
            IAsynchronousFileSystemAccessor fileSystemAccessor,
            IJsonAllTypesSerializer serializer,
            SqliteSettings settings)
        {
            var entityName = typeof(TEntity).Name;
            var pathToDatabase = fileSystemAccessor.CombinePath(settings.PathToDatabaseDirectory, entityName + "-data.sqlite3");
            this.connection = new SQLiteConnectionWithLock(sqLitePlatform,
                new SQLiteConnectionString(pathToDatabase, true, new BlobSerializerDelegate(
                    serializer.SerializeToByteArray,
                    (data, type) => serializer.DeserializeFromStream(new MemoryStream(data), type),
                    (type) => true),
                openFlags: SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex))
            {
                //TraceListener = new MvxTraceListener($"{entityName}-SQL-Queries")
            };
            this.asyncStorage = new SQLiteAsyncConnection(() => this.connection);
            this.logger = logger;
            this.connection.CreateTable<TEntity>();
            this.connection.CreateIndex<TEntity>(entity => entity.Id);
        }

        public SqlitePlainStorage(SQLiteConnectionWithLock storage, ILogger logger)
        {
            this.connection = storage;
            this.logger = logger;
            this.connection.CreateTable<TEntity>();
            this.asyncStorage = new SQLiteAsyncConnection(() => this.connection);
        }

        public virtual TEntity GetById(string id)
        {
            TEntity entity = null;

            using (this.connection.Lock())
                this.connection.RunInTransaction(() => entity = this.connection.Find<TEntity>(x => x.Id == id));

            return entity;
        }

        public virtual async Task<TEntity> GetByIdAsync(string id)
        {
            TEntity entity = null;
            await this.asyncStorage.RunInTransactionAsync(connection => entity = connection.Find<TEntity>(x => x.Id == id));
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

        public void Remove(IEnumerable<TEntity> entities)
        {
            try
            {
                using (this.connection.Lock())
                    foreach (var entity in entities.Where(entity => entity != null))
                        this.connection.Delete(entity);
            }
            catch (SQLiteException ex)
            {
                this.logger.Fatal($"Failed to persist {entities.Count()} entities as batch", ex);
                throw;
            }
        }

        public async Task StoreAsync(TEntity entity)
        {
            await this.StoreAsync(new[] { entity }).ConfigureAwait(false);
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

        public int Count(Expression<Func<TEntity, bool>> predicate)
          => this.RunInTransaction(table => table.Count(predicate));

        public TEntity FirstOrDefault() => this.RunInTransaction(table => table.FirstOrDefault());

        public IReadOnlyCollection<TEntity> LoadAll() => this.RunInTransaction(table => table.ToReadOnlyCollection());

        public async Task<IReadOnlyCollection<TEntity>> LoadAllAsync()
            => await this.RunInTransactionAsync(table => table.ToReadOnlyCollection());

        private TResult RunInTransaction<TResult>(Func<TableQuery<TEntity>, TResult> function)
        {
            TResult result = default(TResult);

            using (this.connection.Lock())
                this.connection.RunInTransaction(() => result = function.Invoke(this.connection.Table<TEntity>()));

            return result;
        }

        private async Task<TResult> RunInTransactionAsync<TResult>(Func<TableQuery<TEntity>, TResult> function)
        {
            TResult result = default(TResult);
            await this.asyncStorage.RunInTransactionAsync(connection => result = function.Invoke(connection.Table<TEntity>()));
            return result;
        }

        public async Task RemoveAllAsync()
        {
            await this.asyncStorage.DeleteAllAsync<TEntity>();
        }

        /*public IReadOnlyCollection<TEntity> Query(
            Expression<Func<TEntity, bool>> wherePredicate,
            Expression<Func<TEntity, bool>> groupingPredicate, 
            Expression<Func<TEntity, bool>> selectPredicate)
        {
            using (this.connection.Lock())
            {
                return this.connection.Table<TEntity>()
                    .Where(wherePredicate)
                    .GroupBy(groupingPredicate)
                    .Select(selectPredicate)
                    .ToReadOnlyCollection();
            }
        }*/


        /*public IReadOnlyCollection<TEntity> QueryOption(Expression<Func<TEntity, bool>> wherePredicate,
            Expression<Func<TEntity, bool>> grouppingPredicate,
            Expression<Func<TEntity, bool>> selectPredicate)
        {
            using (this.storage.Lock())
            {
                return this.storage.Table<TEntity>()
                    .GroupBy(grouppingPredicate)
                    .Select(group => group.Where(wherePredicate).First())
                    .ToReadOnlyCollection();
            }
        }*/


        public void Dispose()
        {
            this.connection.Dispose();
        }
    }
}