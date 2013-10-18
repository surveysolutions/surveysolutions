using System;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.SharedKernels.DataCollection.ReadSide
{
    public interface IVersionedReadSideRepositoryWriter<TEntity> : IReadSideRepositoryWriter<TEntity>
        where TEntity : class, IVersionedView
    {
        TEntity GetById(Guid id, long version);
        void Remove(Guid id, long version);
    }
}
