using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage;
using WB.Core.Infrastructure.Storage.Memory.Implementation;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.Infrastructure.MemoryCachedReadSideStoreTests
{
    [Subject(typeof(MemoryCachedReadSideStorage<>))]
    internal class MemoryCachedReadSideStoreTestContext
    {
        protected static MemoryCachedReadSideStorage<ReadSideRepositoryEntity> CreateMemoryCachedReadSideStore(
            IReadSideStorage<ReadSideRepositoryEntity> readSideStorage = null, int cacheSizeInEntities = 1024, int storeOperationBulkSize = 512)
        {
            return new MemoryCachedReadSideStorage<ReadSideRepositoryEntity>(
                readSideStorage ?? Mock.Of<IReadSideStorage<ReadSideRepositoryEntity>>(),
                Create.ReadSideCacheSettings(cacheSizeInEntities: cacheSizeInEntities, storeOperationBulkSize: storeOperationBulkSize));
        }
    }
    internal class ReadSideRepositoryEntity : IReadSideRepositoryEntity
    {

    }
}
