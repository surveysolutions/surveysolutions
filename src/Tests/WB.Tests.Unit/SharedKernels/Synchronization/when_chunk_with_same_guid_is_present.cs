using System;
using Machine.Specifications;
using Main.DenormalizerStorage;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Synchronization
{
    [Subject(typeof(ReadSideChunkWriter))]
    public class when_chunk_with_same_guid_is_present
    {
        Establish context = () =>
        {
            // arrange
            arId = Guid.NewGuid();
            userId = Guid.NewGuid();
            const string SomeContent1 = "some content1";
            someContent2 = "some content2";
            querableStorageMock = new InMemoryReadSideRepositoryAccessor<SynchronizationDeltaMetaInformation>();
            target = new ReadSideChunkWriter(querableStorageMock, storageReader: querableStorageMock, contentStorage:Mock.Of<IReadSideKeyValueStorage<SynchronizationDeltaContent>>());

            target.StoreChunk(new SyncItem { RootId = arId, Content = SomeContent1, IsCompressed = false }, userId, DateTime.Now);
        };

       Because of = () => target.StoreChunk(new SyncItem() { RootId = arId, Content = someContent2, IsCompressed = false }, userId, DateTime.Now);

       It should_store_both_cunks = () => querableStorageMock.Count().ShouldEqual(2);

       static string someContent2;
       static ReadSideChunkWriter target;
       static Guid arId;
       static Guid userId;
       static InMemoryReadSideRepositoryAccessor<SynchronizationDeltaMetaInformation> querableStorageMock;
    }
}

