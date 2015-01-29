using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Synchronization.ReadSideChunkReaderTests
{
    internal class when_GetChunkMetaDataByTimestamp_is_called_and_provided_timestamp_is_last_one_in_the_storage
    {
        Establish context = () =>
        {
            synchronizationDeltaBehind = new SynchronizationDelta(Guid.NewGuid(), "test", DateTime.Now, null, false, "t",
                "meta", 2);

            var synchronizationDelta = new List<SynchronizationDelta>
            {
                new SynchronizationDelta(Guid.NewGuid(), "test", synchronizationDeltaBehind.Timestamp.AddMinutes(-1), null, false, "t",
                "meta", 1),
                synchronizationDeltaBehind
            }.AsQueryable();

            readSideChunkReader =
                new ReadSideChunkReader(Mock.Of<IQueryableReadSideRepositoryReader<SynchronizationDelta>>(),
                    Mock.Of<IReadSideRepositoryIndexAccessor>(
                        _ => _.Query<SynchronizationDelta>("SynchronizationDeltasByBriefFields") == synchronizationDelta));
        };

        Because of = () =>
            result = readSideChunkReader.GetChunkMetaDataByTimestamp(synchronizationDeltaBehind.Timestamp, Enumerable.Empty<Guid>());

        It should_return_last_created_chunk_before_passed_time_stamp = () =>
            result.Id.ShouldEqual(synchronizationDeltaBehind.PublicKey);

        private static ReadSideChunkReader readSideChunkReader;
        private static SynchronizationDelta synchronizationDeltaBehind;
        private static SynchronizationChunkMeta result;
    }
}
