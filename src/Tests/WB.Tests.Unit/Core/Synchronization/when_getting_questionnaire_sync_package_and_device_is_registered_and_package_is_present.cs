using System;

using Machine.Specifications;

using Moq;

using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.Implementation.SyncManager;
using WB.Core.Synchronization.SyncStorage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Core.Synchronization
{
    internal class when_getting_questionnaire_sync_package_and_device_is_registered_and_package_is_present : SyncManagerTestContext
    {
        Establish context = () =>
        {
            tabletDocument = CreateTabletDocument(deviceId, androidId);
            devices = Mock.Of<IReadSideRepositoryReader<TabletDocument>>(x => x.GetById(deviceId.FormatGuid()) == tabletDocument);
            questionnaireSyncPackageContent = CreateQuestionnaireSyncPackageContent(syncedPackageId, content, meta);

            questionnairePackageContentStore = Mock.Of<IReadSideKeyValueStorage<QuestionnaireSyncPackageContent>>
                (x => x.GetById(syncedPackageId) == questionnaireSyncPackageContent);

            syncManager = CreateSyncManager(devices: devices, questionnaireSyncPackageContentStore: questionnairePackageContentStore);
        };

        Because of = () =>
            package = syncManager.ReceiveQuestionnaireSyncPackage(deviceId, syncedPackageId, userId: Guid.NewGuid());

        It should_return_not_empty_package = () =>
            package.ShouldNotBeNull();

        It should_return_package_with_PackageId_specified = () =>
            package.PackageId.ShouldEqual(syncedPackageId);

        It should_return_package_with_Content_specified = () =>
            package.Content.ShouldEqual(content);

        It should_return_package_with_MetaInfo_specified = () =>
            package.MetaInfo.ShouldEqual(meta);

        private static SyncManager syncManager;
        private static QuestionnaireSyncPackageDto package;

        private const string androidId = "Android";
        private static Guid deviceId = androidId.ToGuid();
        private static TabletDocument tabletDocument;
        private static IReadSideRepositoryReader<TabletDocument> devices;

        private const string syncedPackageId = "some_sync_package_id";
        private const string content = "some_sync_package_content";
        private const string meta = "some_sync_package_meta";
        private static IReadSideKeyValueStorage<QuestionnaireSyncPackageContent> questionnairePackageContentStore;

        private static QuestionnaireSyncPackageContent questionnaireSyncPackageContent;
    }
}