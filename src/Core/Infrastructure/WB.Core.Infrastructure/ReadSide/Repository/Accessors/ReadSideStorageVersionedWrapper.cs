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

            return this.storage.GetById(versionedId);
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
    }
}
