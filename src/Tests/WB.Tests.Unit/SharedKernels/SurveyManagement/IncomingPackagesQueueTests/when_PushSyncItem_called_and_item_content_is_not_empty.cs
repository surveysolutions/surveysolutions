using System;
using System.IO;
using Machine.Specifications;
using Moq;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomingPackagesQueueTests
{
    internal class when_PushSyncItem_called_and_item_content_is_not_empty : IncomingPackagesQueueTestContext
    {
        Establish context = () =>
        {
            mockOfSerializer.Setup(x => x.Deserialize<SyncItem>(contentOfSyncItem)).Returns(syncItem);
            mockOfSerializer.Setup(x => x.Deserialize<InterviewMetaInfo>(decompressedMetaInfo)).Returns(metaInfo);
            mockOfCompressor.Setup(x => x.IsZipStream(Moq.It.IsAny<Stream>())).Returns(true);
            mockOfCompressor.Setup(x => x.DecompressString(syncItem.MetaInfo)).Returns(decompressedMetaInfo);
            mockOfCompressor.Setup(x => x.DecompressString(syncItem.Content)).Returns(decompressedContent);
            incomingSyncPackagesService = CreateIncomingPackagesQueue(serializer: mockOfSerializer.Object,
                archiver: mockOfCompressor.Object, interviewPackageStorage: mockOfPackagesAccessor.Object);
        };

        Because of = () => incomingSyncPackagesService.StoreOrProcessPackage(contentOfSyncItem);

        It should_deserialize_sync_item = () =>
          mockOfSerializer.Verify(x => x.Deserialize<SyncItem>(contentOfSyncItem), Times.Once);

        It should_deserialize_meta_info_by_interview = () =>
          mockOfSerializer.Verify(x => x.Deserialize<InterviewMetaInfo>(decompressedMetaInfo), Times.Once);

        It should_decompress_intefview_meta_info_and_events = () =>
          mockOfCompressor.Verify(x => x.DecompressString(Moq.It.IsAny<string>()), Times.Exactly(2));

        It should_store_interview_package_to_storage = () =>
          mockOfPackagesAccessor.Verify(x => x.Store(Moq.It.IsAny<InterviewPackage>(), null), Times.Once);

        private static IncomingSyncPackagesService incomingSyncPackagesService;

        private static readonly SyncItem syncItem = new SyncItem()
        {
            MetaInfo = "meta info by interview",
            Content = "interview events"
        };
        private static readonly InterviewMetaInfo metaInfo = new InterviewMetaInfo()
        {
            ResponsibleId = Guid.Parse("11111111111111111111111111111111"),
            TemplateId = Guid.Parse("22222222222222222222222222222222"),
            TemplateVersion = 111,
            Status = (int)InterviewStatus.Restarted,
            CreatedOnClient = true
        };
        private static readonly Mock<IJsonAllTypesSerializer> mockOfSerializer = new Mock<IJsonAllTypesSerializer>();
        private static readonly Mock<IArchiveUtils> mockOfCompressor = new Mock<IArchiveUtils>();
        private static readonly Mock<IPlainStorageAccessor<InterviewPackage>> mockOfPackagesAccessor = new Mock<IPlainStorageAccessor<InterviewPackage>>();
        private static string contentOfSyncItem = "content of sync item";
        private static string decompressedMetaInfo = "decompressed meta info";
        private static string decompressedContent = "decompressed content";
    }
}
