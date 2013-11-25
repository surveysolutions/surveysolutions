using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.ReadSide
{
    public class ReadSideStorageStrategy<T> : IStorageStrategy<T> where T : class, IReadSideRepositoryEntity
    {
        private readonly IReadSideRepositoryWriter<T> writer;

        public ReadSideStorageStrategy(IReadSideRepositoryWriter<T> writer)
        {
            this.writer = writer;
        }

        public T Select(Guid id)
        {
            return writer.GetById(id);
        }

        public void AddOrUpdate(T projection, Guid id)
        {
            writer.Store(projection, id);
        }

        public void Delete(T projection, Guid id)
        {
            writer.Remove(id);
        }
    }
}
