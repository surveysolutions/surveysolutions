using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Memory.Implementation;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.Infrastructure.MemoryCachedReadSideStoreTests
{
    [Subject(typeof(MemoryCachedReadSideStore<>))]
    internal class MemoryCachedReadSideStoreTestContext
    {
        protected static MemoryCachedReadSideStore<ReadSideRepositoryEntity> CreateMemoryCachedReadSideStore(IReadSideStorage<ReadSideRepositoryEntity> readSideStorage =null)
        {
            return new MemoryCachedReadSideStore<ReadSideRepositoryEntity>(
                readSideStorage ?? Mock.Of<IReadSideStorage<ReadSideRepositoryEntity>>(),
                new ReadSideStoreMemoryCacheSettings(256, 128));
        }
    }
    public class ReadSideRepositoryEntity : IReadSideRepositoryEntity
    {

    }
}
