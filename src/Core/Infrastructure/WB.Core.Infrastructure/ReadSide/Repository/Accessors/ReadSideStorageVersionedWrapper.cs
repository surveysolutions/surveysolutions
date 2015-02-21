using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.ReadSide.Repository.Accessors
{
    public class ReadSideStorageVersionedWrapper<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IReadSideStorage<TEntity> storage;

        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        public ReadSideStorageVersionedWrapper(IReadSideStorage<TEntity> storage)
        {
            this.storage = storage;
        }

        public TEntity Get(string id, long version)
        {
            string versionedId = GetVersionedId(id, version);

            var newStyleResult = this.storage.GetById(versionedId);
            if (newStyleResult != null)
                return newStyleResult;

#warning this code provide backward compatablity with old VersionedReadSideRepositoryWriter removed with commit https://bitbucket.org/wbcapi/surveysolutions/commits/8e26ffa2
            var entity = this.storage.GetById(GetOldVersionedKey(id, version));
            if (entity != null)
                return entity;

            entity = this.storage.GetById(id);

            if (entity == null)
                return null;
            try
            {
#warning this code is need to restore IVersionedView(deleted interface) prperty Version commit https://bitbucket.org/wbcapi/surveysolutions/commits/8e26ffa2
                var entityVersion = (long)entity.GetType().GetProperty("Version").GetValue(entity, null);
                if (entityVersion == version)
                    return entity;
            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Error on getting entity with id: {0}, version: {1}", id, version), e);
            }
            return null;
        }

        public void Remove(string id, long version)
        {
            string versionedId = GetVersionedId(id, version);

            this.storage.Remove(versionedId);
        }

        public void Store(TEntity view, string id, long version)
        {
            string versionedId = GetVersionedId(id, version);

            this.storage.Store(view, versionedId);
        }

        private static string GetVersionedId(string id, long version)
        {
            return string.Format("{0}${1}", id, version);
        }

        public static string GetOldVersionedKey(string id, long version)
        {
            return String.Format("{0}-{1}", id, version);
        }
    }
}
