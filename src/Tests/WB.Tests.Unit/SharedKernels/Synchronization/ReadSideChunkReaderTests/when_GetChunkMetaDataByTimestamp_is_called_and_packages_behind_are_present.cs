using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Synchronization.ReadSideChunkReaderTests
{
    internal class when_GetChunkMetaDataByTimestamp_is_called_and_packages_behind_are_present
    {
        Establish context = () =>
        {
            _synchronizationDeltaMetaInformationBehind = new SynchronizationDeltaMetaInformation(Guid.NewGuid(),
                DateTime.Now, null, "q", 2, 2, 1);

            var synchronizationDelta = new List<SynchronizationDeltaMetaInformation>()
            {
                new SynchronizationDeltaMetaInformation(Guid.NewGuid(), _synchronizationDeltaMetaInformationBehind.Timestamp, null, "q", 1,2,1),
                _synchronizationDeltaMetaInformationBehind
            }.AsQueryable();

            readSideChunkReader =
                new ReadSideChunkReader(Mock.Of<IQueryableReadSideRepositoryReader<SynchronizationDeltaMetaInformation>>(),
                    Mock.Of<IReadSideRepositoryIndexAccessor>(
                        _ => _.Query<SynchronizationDeltaMetaInformation>("SynchronizationDeltasByBriefFields") == synchronizationDelta), Mock.Of<IReadSideKeyValueStorage<SynchronizationDeltaContent>>());
        };

        Because of = () =>
            result = readSideChunkReader.GetChunkMetaDataByTimestamp(_synchronizationDeltaMetaInformationBehind.Timestamp.AddMinutes(1), Enumerable.Empty<Guid>());

        It should_return_last_created_chunk_before_passed_time_stamp = () =>
            result.Id.ShouldEqual(_synchronizationDeltaMetaInformationBehind.PublicKey);

        private static ReadSideChunkReader readSideChunkReader;
        private static SynchronizationDeltaMetaInformation _synchronizationDeltaMetaInformationBehind;
        private static SynchronizationChunkMeta result;
    }
}
