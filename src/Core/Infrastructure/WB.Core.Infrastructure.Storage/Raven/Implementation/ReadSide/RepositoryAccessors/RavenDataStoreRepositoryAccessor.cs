using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors
{
    internal class RavenDataStoreRepositoryAccessor<TEntity> : IReadSideKeyValueStorage<TEntity>, IReadSideRepositoryCleaner
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly RavenReadSideRepositoryWriter<TEntity> readSideRepositoryWriter;
        private readonly IAdditionalDataService<TEntity> additionalDataService;
        private bool isCacheEnabled = false;
        public RavenDataStoreRepositoryAccessor(RavenReadSideRepositoryWriter<TEntity> readSideRepositoryWriter)
        {
            this.readSideRepositoryWriter = readSideRepositoryWriter;
        }
        public RavenDataStoreRepositoryAccessor(RavenReadSideRepositoryWriter<TEntity> readSideRepositoryWriter, IAdditionalDataService<TEntity> additionalDataService)
            : this(readSideRepositoryWriter)
        {
            this.additionalDataService = additionalDataService;
        }

        public TEntity GetById(string id)
        {
            if (!isCacheEnabled)
            {
                if (additionalDataService != null)
                    additionalDataService.CheckAdditionalRepository(id);
            }
            return readSideRepositoryWriter.GetById(id);
        }

        public void Remove(string id)
        {
           readSideRepositoryWriter.Remove(id);
        }

        public void Store(TEntity view, string id)
        {
          readSideRepositoryWriter.Store(view, id);
        }

        public void Clear()
        {
            readSideRepositoryWriter.Clear();
        }

        public Type ViewType
        {
            get { return typeof(TEntity); }
        }

        public string GetReadableStatus()
        {
            return "Raven Key/Value O_o";
        }
    }
}
