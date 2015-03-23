using System;
using System.Linq;

using Machine.Specifications;

using Moq;

using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.Implementation.ReadSide.Indexes;
using WB.Core.Synchronization.Implementation.SyncManager;
using WB.Core.Synchronization.SyncStorage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Core.Synchronization
{
    internal class when_getting_questionnaire_ids_and_device_is_registered_and_last_package_id_is_not_empty_but_package_is_absent : SyncManagerTestContext
    {
        Establish context = () =>
        {
            tabletDocument = CreateTabletDocument(deviceId, androidId);
            devices = Mock.Of<IReadSideRepositoryReader<TabletDocument>>(x => x.GetById(deviceId.FormatGuid()) == tabletDocument);

            lastSyncedPackageId = "22222222222222222222222222222222_1$2";

            indexAccessorMock = new Mock<IReadSideRepositoryIndexAccessor>();

            indexAccessorMock.Setup(x => x.Query<QuestionnaireSyncPackageMeta>(questionnireQueryIndexName))
                .Returns(Enumerable.Empty<QuestionnaireSyncPackageMeta>().AsQueryable());

            syncManager = CreateSyncManager(devices: devices, indexAccessor: indexAccessorMock.Object);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                syncManager.GetQuestionnairePackageIdsWithOrder(userId, deviceId, lastSyncedPackageId));

        It should_throw_SyncPackageNotFoundException_exception = () =>
            exception.ShouldBeOfExactType<SyncPackageNotFoundException>();

        It should_throw_exception_with_message_containting__package_not_found_on_server__ = () =>
            new[] { "sync package with id", lastSyncedPackageId, "was not found on server" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));

        private static SyncManager syncManager;
        private static Exception exception;

        private static string androidId = "Android";
        private static Guid deviceId = androidId.ToGuid();
        private static TabletDocument tabletDocument;
        private static IReadSideRepositoryReader<TabletDocument> devices;

        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static string lastSyncedPackageId = "sync package id";

        private static Mock<IReadSideRepositoryIndexAccessor> indexAccessorMock;
        private static readonly string questionnireQueryIndexName = typeof(QuestionnaireSyncPackagesByBriefFields).Name;
    }
}