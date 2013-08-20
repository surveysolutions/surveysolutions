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
    internal class VersionedReadSideRepositoryReader<TEntity> : IVersionedReadSideRepositoryReader<TEntity>
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IReadSideRepositoryReader<TEntity> internalRepositoryReader;

        public VersionedReadSideRepositoryReader(IReadSideRepositoryReader<TEntity> internalRepositoryReader)
        {
            this.internalRepositoryReader = internalRepositoryReader;
        }

        public int Count()
        {
            return this.internalRepositoryReader.Count();
        }

        public TEntity GetById(Guid id)
        {
            return this.internalRepositoryReader.GetById(id);
        }

        public TEntity GetById(Guid id, long version)
        {
            return this.internalRepositoryReader.GetById(id.Combine(version));
        }
    }
}
