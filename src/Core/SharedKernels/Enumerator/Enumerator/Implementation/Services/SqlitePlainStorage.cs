using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite.Net;
using SQLite.Net.Async;
using SQLite.Net.Interop;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class SqlitePlainStorage<TEntity> : IAsyncPlainStorage<TEntity> where TEntity: class, IPlainStorageEntity
    {
        private readonly SQLiteAsyncConnection asyncStorage;
        private readonly SQLiteConnectionWithLock storage;
        protected readonly ILogger logger;

        public SqlitePlainStorage(ISQLitePlatform sqLitePlatform, ILogger logger,
            IAsynchronousFileSystemAccessor fileSystemAccessor, ISerializer serializer, SqliteSettings settings)
        {
            var pathToDatabase = fileSystemAccessor.CombinePath(settings.PathToDatabaseDirectory, "data.mdb");
            this.storage = new SQLiteConnectionWithLock(sqLitePlatform,
                new SQLiteConnectionString(pathToDatabase, true, new BlobSerializerDelegate(
                    serializer.SerializeToByteArray,
                    (data, type) => serializer.DeserializeFromStream(new MemoryStream(data), type),
                    (type) => true)));
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
            TEntity entity = await Task.FromResult(this.GetById(id));

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
            catch (Exception ex)
            {
                this.logger.Fatal(ex.Message, ex);
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
                        if (this.GetById(entity.Id) != null)
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
            catch (Exception ex)
            {
                this.logger.Fatal(ex.Message, ex);
            }
        }

        public TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> query)
        {
            TResult result = default(TResult);
            this.storage.RunInTransaction(() => result = query.Invoke(this.storage.Table<TEntity>().AsQueryable()));
            return result;
        }
    }
}