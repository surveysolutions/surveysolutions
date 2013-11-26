using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide
{
    public class ReadSideRepositoryWriterWithSequence<T> : IReadSideRepositoryWriter<T> where T : class, IReadSideRepositoryEntity
    {
        public T GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Remove(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Store(T view, Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
