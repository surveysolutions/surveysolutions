using System;
using Machine.Specifications;
using Main.DenormalizerStorage;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Synchronization
{
    [Subject(typeof(ReadSideChunkWriter))]
    public class when_content_is_not_empty
    {
        Establish context = () =>
        {
            // arrange
            arId = Guid.NewGuid();
            userId = Guid.NewGuid();
            someContent = "some content";
            querableStorageMock = new InMemoryReadSideRepositoryAccessor<SynchronizationDeltaMetaInformation>();
            contentStorageMock= new InMemoryReadSideRepositoryAccessor<SynchronizationDeltaContent>();
            target = new ReadSideChunkWriter(querableStorageMock, contentStorageMock, Mock.Of<IReadSideKeyValueStorage<SynchronizationDeltasCounter>>());
        };

        Because of = () => target.StoreChunk(new SyncItem() { RootId = arId, Content = someContent, IsCompressed = false }, userId, DateTime.Now);

        It should_save_content = () => contentStorageMock.GetById(arId.FormatGuid() + "$0").Content.ShouldEqual(someContent);

        static ReadSideChunkWriter target;
        static InMemoryReadSideRepositoryAccessor<SynchronizationDeltaMetaInformation> querableStorageMock;
        static InMemoryReadSideRepositoryAccessor<SynchronizationDeltaContent> contentStorageMock;
        static Guid arId;
        static Guid userId;
        static string someContent;
    }
}

