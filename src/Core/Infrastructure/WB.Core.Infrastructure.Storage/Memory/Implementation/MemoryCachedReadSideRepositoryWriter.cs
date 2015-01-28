using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Memory.Implementation
{
    internal class MemoryCachedReadSideRepositoryWriter<TEntity> : MemoryCachedReadSideStore<TEntity>,
        IReadSideRepositoryWriter<TEntity> where TEntity : class, IReadSideRepositoryEntity
    {
        public MemoryCachedReadSideRepositoryWriter(IReadSideRepositoryWriter<TEntity> readSideStorage)
            : base(readSideStorage)
        {
        }
    }
}
