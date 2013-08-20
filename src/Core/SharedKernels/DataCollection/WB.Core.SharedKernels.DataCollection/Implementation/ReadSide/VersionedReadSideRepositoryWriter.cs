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
    internal class VersionedReadSideRepositoryWriter<TEntity> : IVersionedReadSideRepositoryWriter<TEntity> where TEntity : class, IReadSideRepositoryEntity
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
            internalRepositoryWroter.Store(view, id);
        }

        public TEntity GetById(Guid id, long version)
        {
            return internalRepositoryWroter.GetById(id.Combine(version));
        }

        public void Remove(Guid id, long version)
        {
            internalRepositoryWroter.Remove(id.Combine(version));
        }

        public void Store(TEntity view, Guid id, long version)
        {
            internalRepositoryWroter.Store(view, id.Combine(version));
        }
    }
}
