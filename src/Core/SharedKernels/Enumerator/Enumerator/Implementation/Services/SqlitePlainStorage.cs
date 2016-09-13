using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SQLite;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class SqlitePlainStorage<TEntity> : IAsyncPlainStorage<TEntity>
        where TEntity : class, IPlainStorageEntity, new()
    {
        protected readonly SQLiteConnection connection;
        private readonly ILogger logger;

        public SqlitePlainStorage(ILogger logger,
            IAsynchronousFileSystemAccessor fileSystemAccessor,
            SqliteSettings settings)
        {
            var entityName = typeof(TEntity).Name;
            var pathToDatabase = fileSystemAccessor.CombinePath(settings.PathToDatabaseDirectory, entityName + "-data.sqlite3");
            this.connection = new SQLiteConnection(pathToDatabase, 
                openFlags: SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex);
            
            this.logger = logger;
        }

        public SqlitePlainStorage(SQLiteConnection storage, ILogger logger)
        {
            this.connection = storage;
            this.logger = logger;
            this.connection.CreateTable<TEntity>();
        }

        public virtual TEntity GetById(string id)
        {
            return this.connection.Find<TEntity>(id);
        }

        public void Remove(string id)
        {
            this.connection.RunInTransaction(() => this.connection.Delete<TEntity>(id));
        }

        public void Remove(IEnumerable<TEntity> entities)
        {
            try
            {
                this.connection.RunInTransaction(() =>
                {
                    foreach (var entity in entities.Where(entity => entity != null))
                        this.connection.Delete(entity);
                });
            }
            catch (SQLiteException ex)
            {
                this.logger.Fatal($"Failed to persist {entities.Count()} entities as batch", ex);
                throw;
            }
        }

        public void Store(TEntity entity)
        {
            this.Store(new[] {entity});
        }

        public virtual void Store(IEnumerable<TEntity> entities)
        {
            try
            {
                this.connection.RunInTransaction(() =>
                {
                    foreach (var entity in entities.Where(entity => entity != null))
                    {
                        var isEntityExists = connection.Table<TEntity>().FirstOrDefault(x => x.Id == entity.Id) != null;

                        if (isEntityExists)
                        {
                            connection.Update(entity);
                        }
                        else
                        {
                            connection.Insert(entity);
                        }
                    }
                });

            }
            catch (SQLiteException ex)
            {
                this.logger.Fatal($"Failed to persist {entities.Count()} entities as batch", ex);
                throw;
            }
        }

        public IReadOnlyCollection<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
            => this.RunInTransaction(table => table.Where(predicate).ToReadOnlyCollection());

        public int Count(Expression<Func<TEntity, bool>> predicate)
          => this.RunInTransaction(table => table.Count(predicate));

        public TEntity FirstOrDefault() => this.RunInTransaction(table => table.FirstOrDefault());

        public IReadOnlyCollection<TEntity> LoadAll() => this.RunInTransaction(table => table.ToReadOnlyCollection());

        private TResult RunInTransaction<TResult>(Func<TableQuery<TEntity>, TResult> function)
        {
            TResult result = default(TResult);
            this.connection.RunInTransaction(() => result = function.Invoke(this.connection.Table<TEntity>()));
            return result;
        }

        public IReadOnlyCollection<TEntity> FixedQuery(Expression<Func<TEntity, bool>> wherePredicate, Expression<Func<TEntity, int>> orderPredicate, int takeCount)
            => this.RunInTransaction(table => table.Where(wherePredicate).OrderBy(orderPredicate).Take(takeCount).ToReadOnlyCollection());
        
        public void RemoveAll()
        {
            this.connection.DeleteAll<TEntity>();
        }

        public void Dispose()
        {
            this.connection.Dispose();
        }
    }
}