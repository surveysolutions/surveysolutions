using System;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.SharedKernels.DataCollection.Implementation.ReadSide
{
    internal class VersionedReadSideRepositoryReader<TEntity> : IVersionedReadSideRepositoryReader<TEntity>
        where TEntity : class, IVersionedView
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

        public TEntity GetById(string id)
        {
            return this.internalRepositoryReader.GetById(id);
        }

        public TEntity GetById(string id, long version)
        {
            var entity = internalRepositoryReader.GetById(RepositoryKeysHelper.GetVersionedKey(id, version));
            if (entity != null)
                return entity;
            entity = internalRepositoryReader.GetById(id);
            if (entity == null)
                return null;
            if (entity.Version == version)
                return entity;
            return null;
        }
    }

   
}
