using System;
using System.Collections;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Synchronization.SimpleSynchronizationDataStorageTests
{
    internal class when_getting_chunk_info_by_timestamp : SimpleSynchronizationDataStorageTestContext
    {
        Establish context = () =>
        {
            timestamp = DateTime.MinValue;
            chunkStorageReader = new Mock<IChunkReader>();
            simpleSynchronizationDataStorage = GetSimpleSynchronizationDataStorage(chunkStorageReader: chunkStorageReader.Object);
        };

        private Because of = () => simpleSynchronizationDataStorage.GetChunkInfoByTimestamp(timestamp, new Guid());

        It should_get_chunk_meta_data_by_timestamp = () =>
            chunkStorageReader.Verify(x => x.GetChunkMetaDataByTimestamp(timestamp, Moq.It.IsAny<IEnumerable<Guid>>()), Times.Once);
        
        private static SimpleSynchronizationDataStorage simpleSynchronizationDataStorage;
        private static Mock<IChunkReader> chunkStorageReader;

        private static DateTime timestamp;
    }
}
