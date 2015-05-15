using System;
using System.Collections.Generic;
using System.Globalization;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Memory.Implementation;
using WB.Tests.Unit.Infrastructure.MemoryCachedReadSideStoreTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.MemoryCachedReadSideRepositoryWriterTests
{
    [Subject(typeof(MemoryCachedReadSideRepositoryWriter<>))]
    public class when_inserting_last_item_in_store
    {
        Establish context = () =>
        {
            batchedWriter = new Mock<IReadSideRepositoryWriter<ReadSideRepositoryEntity>>();
            memoryWriter = new MemoryCachedReadSideRepositoryWriter<ReadSideRepositoryEntity>(batchedWriter.Object, new ReadSideStoreMemoryCacheSettings(256, 128));

            memoryWriter.EnableCache();
            for (int i = 0; i < 255; i++)
            {
                memoryWriter.Store(new ReadSideRepositoryEntity(), i.ToString(CultureInfo.InvariantCulture));
                if (i == 1)
                {
                    memoryWriter.Store(null, i.ToString(CultureInfo.InvariantCulture));
                }
            }
        };

        Because of = () => memoryWriter.Store(new ReadSideRepositoryEntity(), "256");

        It should_remove_deleted_data_from_store = () => batchedWriter.Verify(x => x.Remove("1"), Times.Once);

        It should_push_data_to_store = () => 
            batchedWriter.Verify(x => x.BulkStore(Moq.It.IsAny<List<Tuple<ReadSideRepositoryEntity, string>>>()), Times.Once);

        static MemoryCachedReadSideRepositoryWriter<ReadSideRepositoryEntity> memoryWriter;
        static Mock<IReadSideRepositoryWriter<ReadSideRepositoryEntity>> batchedWriter;
    }
}

