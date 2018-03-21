using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Infrastructure.Native.Storage.Memory.Implementation;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.MemoryCachedReadSideStoreTests
{
    [NUnit.Framework.TestOf(typeof(MemoryCachedReadSideStorage<>))]
    internal class MemoryCachedReadSideStoreTestContext
    {
        protected static MemoryCachedReadSideStorage<ReadSideRepositoryEntity> CreateMemoryCachedReadSideStore(
            IReadSideStorage<ReadSideRepositoryEntity> readSideStorage = null, int cacheSizeInEntities = 1024, int storeOperationBulkSize = 512)
        {
            return new MemoryCachedReadSideStorage<ReadSideRepositoryEntity>(
                readSideStorage ?? Mock.Of<IReadSideStorage<ReadSideRepositoryEntity>>(),
                Create.Entity.ReadSideCacheSettings(cacheSizeInEntities: cacheSizeInEntities, storeOperationBulkSize: storeOperationBulkSize));
        }
    }
    internal class ReadSideRepositoryEntity : IReadSideRepositoryEntity
    {

    }
}
