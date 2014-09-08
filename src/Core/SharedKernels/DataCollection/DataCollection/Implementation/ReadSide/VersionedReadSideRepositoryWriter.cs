using System;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.SharedKernels.DataCollection.Implementation.ReadSide
{
    internal class VersionedReadSideRepositoryWriter<TEntity> : IVersionedReadSideRepositoryWriter<TEntity> where TEntity : class, IVersionedView
    {
        private readonly IReadSideRepositoryWriter<TEntity> internalRepositoryWroter;

        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        public VersionedReadSideRepositoryWriter(IReadSideRepositoryWriter<TEntity> internalRepositoryWroter)
        {
            this.internalRepositoryWroter = internalRepositoryWroter;
        }

        public TEntity GetById(string id)
        {
            return internalRepositoryWroter.GetById(id);
        }

        public void Remove(string id)
        {
            internalRepositoryWroter.Remove(id);
        }

        public void Store(TEntity view, string id)
        {
            TEntity previousEntity = null;
            try
            {
                previousEntity = internalRepositoryWroter.GetById(id);
            }
            catch (Exception e)
            {
                Logger.Error("error during restore QuestionnaireRosterStructure", e);
            }
            if (previousEntity != null)
            {
                internalRepositoryWroter.Store(previousEntity, RepositoryKeysHelper.GetVersionedKey(id, previousEntity.Version));
            }
            internalRepositoryWroter.Store(view, id);
        }

        public TEntity GetById(string id, long version)
        {
            var entity = internalRepositoryWroter.GetById(RepositoryKeysHelper.GetVersionedKey(id, version));
            if (entity != null)
                return entity;
            entity = internalRepositoryWroter.GetById(id);
            if (entity == null)
                return null;
            if (entity.Version == version)
                return entity;
            return null;
        }

        public void Remove(string id, long version)
        {
            internalRepositoryWroter.Remove(RepositoryKeysHelper.GetVersionedKey(id, version));
            TEntity currentEntity = internalRepositoryWroter.GetById(id);

            if (currentEntity != null && currentEntity.Version == version)
            {
                internalRepositoryWroter.Remove(id);

                var newVersion = version - 1;

                while (newVersion > 0)
                {
                    var previousVersion = internalRepositoryWroter.GetById(RepositoryKeysHelper.GetVersionedKey(id, version));
                    if (previousVersion != null)
                    {
                        internalRepositoryWroter.Store(previousVersion, id);
                        break;
                    }
                    newVersion--;
                }
            }
        }
    }
}
