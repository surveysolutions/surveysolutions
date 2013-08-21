using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Utility;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;

namespace WB.Core.SharedKernels.DataCollection.Implementation.ReadSide
{
    internal class VersionedReadSideRepositoryWriter<TEntity> : IVersionedReadSideRepositoryWriter<TEntity> where TEntity : class, IVersionedView
    {
        private readonly IReadSideRepositoryWriter<TEntity> internalRepositoryWroter;

        public VersionedReadSideRepositoryWriter(IReadSideRepositoryWriter<TEntity> internalRepositoryWroter)
        {
            this.internalRepositoryWroter = internalRepositoryWroter;
        }

        public TEntity GetById(Guid id)
        {
            return internalRepositoryWroter.GetById(id);
        }

        public void Remove(Guid id)
        {
            internalRepositoryWroter.Remove(id);
        }

        public void Store(TEntity view, Guid id)
        {
            var previousEntity = internalRepositoryWroter.GetById(id);
            if (previousEntity != null)
            {
                internalRepositoryWroter.Store(previousEntity, id.Combine(previousEntity.Version));
            }
            internalRepositoryWroter.Store(view, id);
        }

        public TEntity GetById(Guid id, long version)
        {
       //     return internalRepositoryWroter.GetById(id.Combine(version));
            var entity = internalRepositoryWroter.GetById(id.Combine(version));
            if (entity != null)
                return entity;
            entity = internalRepositoryWroter.GetById(id);
            if (entity == null)
                return null;
            if (entity.Version == version)
                return entity;
            return null;
        }

        public void Remove(Guid id, long version)
        {
            internalRepositoryWroter.Remove(id.Combine(version));
        }
    }
}
