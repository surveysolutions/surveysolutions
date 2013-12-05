using System;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Implementation.StorageStrategy
{
    internal class ReadSideStorageStrategy<T> : IStorageStrategy<T> where T : class, IReadSideRepositoryEntity
    {
        private readonly IReadSideRepositoryWriter<T> writer;

        public ReadSideStorageStrategy(IReadSideRepositoryWriter<T> writer)
        {
            this.writer = writer;
        }

        public T Select(Guid id)
        {
            return this.writer.GetById(id);
        }

        public void AddOrUpdate(T projection, Guid id)
        {
            this.writer.Store(projection, id);
        }

        public void Delete(T projection, Guid id)
        {
            this.writer.Remove(id);
        }
    }
}
