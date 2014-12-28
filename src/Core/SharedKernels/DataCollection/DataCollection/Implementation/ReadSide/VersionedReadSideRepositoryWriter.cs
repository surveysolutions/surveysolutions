using System;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.SharedKernels.DataCollection.Implementation.ReadSide
{
    internal class VersionedReadSideRepositoryWriter<TEntity> : IReadSideRepositoryCleaner, IReadSideRepositoryWriter, IVersionedReadSideRepositoryWriter<TEntity> where TEntity : class, IVersionedView
    {
        private readonly IReadSideRepositoryWriter<TEntity> internalRepositoryWriter;

        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        public VersionedReadSideRepositoryWriter(IReadSideRepositoryWriter<TEntity> internalRepositoryWriter)
        {
            this.internalRepositoryWriter = internalRepositoryWriter;
        }

        public TEntity GetById(string id)
        {
            return this.internalRepositoryWriter.GetById(id);
        }

        public void Remove(string id)
        {
            this.internalRepositoryWriter.Remove(id);
        }

        public void Store(TEntity view, string id)
        {
            TEntity previousEntity = null;
            try
            {
                previousEntity = this.internalRepositoryWriter.GetById(id);
            }
            catch (Exception e)
            {
                Logger.Error("error during restore QuestionnaireRosterStructure", e);
            }
            if (previousEntity != null)
            {
                this.internalRepositoryWriter.Store(previousEntity, RepositoryKeysHelper.GetVersionedKey(id, previousEntity.Version));
            }
            this.internalRepositoryWriter.Store(view, id);
        }

        public TEntity GetById(string id, long version)
        {
            var entity = this.internalRepositoryWriter.GetById(RepositoryKeysHelper.GetVersionedKey(id, version));
            if (entity != null)
                return entity;
            entity = this.internalRepositoryWriter.GetById(id);
            if (entity == null)
                return null;
            if (entity.Version == version)
                return entity;
            return null;
        }

        public void Remove(string id, long version)
        {
            this.internalRepositoryWriter.Remove(RepositoryKeysHelper.GetVersionedKey(id, version));
            TEntity currentEntity = this.internalRepositoryWriter.GetById(id);

            if (currentEntity != null && currentEntity.Version == version)
            {
                this.internalRepositoryWriter.Remove(id);

                var newVersion = version - 1;

                while (newVersion > 0)
                {
                    var previousVersion = this.internalRepositoryWriter.GetById(RepositoryKeysHelper.GetVersionedKey(id, version));
                    if (previousVersion != null)
                    {
                        this.internalRepositoryWriter.Store(previousVersion, id);
                        break;
                    }
                    newVersion--;
                }
            }
        }

        public void Clear()
        {
            var readSideRepositoryCleaner = internalRepositoryWriter as IReadSideRepositoryCleaner;
            if(readSideRepositoryCleaner!=null)
                readSideRepositoryCleaner.Clear();
        }

        public void EnableCache()
        {
            var readSideRepositoryWriter = internalRepositoryWriter as IReadSideRepositoryWriter;
            if (readSideRepositoryWriter != null)
                readSideRepositoryWriter.EnableCache();
        }

        public void DisableCache()
        {
            var readSideRepositoryWriter = internalRepositoryWriter as IReadSideRepositoryWriter;
            if (readSideRepositoryWriter != null)
                readSideRepositoryWriter.DisableCache();
        }

        public string GetReadableStatus()
        {
            var readSideRepositoryWriter = internalRepositoryWriter as IReadSideRepositoryWriter;
            if (readSideRepositoryWriter != null)
                return readSideRepositoryWriter.GetReadableStatus();
            return "";
        }

        public Type ViewType { get { return typeof (TEntity); } }
    }
}
