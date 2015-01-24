using System;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.SharedKernels.DataCollection.Implementation.ReadSide
{
    internal class VersionedReadSideKeyValueStorage<TEntity> : IVersionedReadSideKeyValueStorage<TEntity>, IReadSideRepositoryCleaner, IReadSideRepositoryWriter
        where TEntity : class, IVersionedView
    {
        private readonly IReadSideKeyValueStorage<TEntity> internalRepositoryReader;

        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        public VersionedReadSideKeyValueStorage(IReadSideKeyValueStorage<TEntity> internalRepositoryReader)
        {
            this.internalRepositoryReader = internalRepositoryReader;
        }

        public void Remove(string id)
        {
            this.internalRepositoryReader.Remove(id);
        }

        public void Remove(string id, long version)
        {
            this.internalRepositoryReader.Remove(RepositoryKeysHelper.GetVersionedKey(id, version));
            TEntity currentEntity = this.internalRepositoryReader.GetById(id);

            if (currentEntity != null && currentEntity.Version == version)
            {
                this.internalRepositoryReader.Remove(id);

                var newVersion = version - 1;

                while (newVersion > 0)
                {
                    var previousVersion = this.internalRepositoryReader.GetById(RepositoryKeysHelper.GetVersionedKey(id, version));
                    if (previousVersion != null)
                    {
                        this.internalRepositoryReader.Store(previousVersion, id);
                        break;
                    }
                    newVersion--;
                }
            }
        }

        public void Store(TEntity view, string id)
        {
            TEntity previousEntity = null;
            try
            {
                previousEntity = this.internalRepositoryReader.GetById(id);
            }
            catch (Exception e)
            {
                Logger.Error("error during restore QuestionnaireRosterStructure", e);
            }
            if (previousEntity != null)
            {
                this.internalRepositoryReader.Store(previousEntity, RepositoryKeysHelper.GetVersionedKey(id, previousEntity.Version));
            }
            this.internalRepositoryReader.Store(view, id);
        }

        public TEntity GetById(string id)
        {
            return this.internalRepositoryReader.GetById(id);
        }

        public TEntity GetById(string id, long version)
        {
            var entity = internalRepositoryReader.GetById(RepositoryKeysHelper.GetVersionedKey(id, version));
            if (entity != null)
                return entity;
            entity = internalRepositoryReader.GetById(id);
            if (entity == null)
                return null;
            if (entity.Version == version)
                return entity;
            return null;
        }

        public void Clear()
        {
            var readSideRepositoryCleaner = internalRepositoryReader as IReadSideRepositoryCleaner;
            if (readSideRepositoryCleaner != null)
                readSideRepositoryCleaner.Clear();
        }

        public void EnableCache()
        {
            var readSideRepositoryWriter = internalRepositoryReader as IReadSideRepositoryWriter;
            if (readSideRepositoryWriter != null)
                readSideRepositoryWriter.EnableCache();
        }

        public void DisableCache()
        {
            var readSideRepositoryWriter = internalRepositoryReader as IReadSideRepositoryWriter;
            if (readSideRepositoryWriter != null)
                readSideRepositoryWriter.DisableCache();
        }

        public string GetReadableStatus()
        {
            var readSideRepositoryWriter = internalRepositoryReader as IReadSideRepositoryWriter;
            if (readSideRepositoryWriter != null)
                return readSideRepositoryWriter.GetReadableStatus();
            return "";
        }

        public Type ViewType { get { return typeof(TEntity); } }
    }
}
