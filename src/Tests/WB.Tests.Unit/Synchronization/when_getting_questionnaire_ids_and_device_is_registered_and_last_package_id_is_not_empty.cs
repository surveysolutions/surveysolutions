using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.Implementation.SyncManager;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Synchronization
{
    internal class when_getting_questionnaire_ids_and_device_is_registered_and_last_package_id_is_not_empty : SyncManagerTestContext
    {
        Establish context = () =>
        {
            tabletDocument = CreateTabletDocument(deviceId, androidId);
            devices = Mock.Of<IReadSideRepositoryReader<TabletDocument>>(x => x.GetById(deviceId.FormatGuid()) == tabletDocument);

            questionnaireSyncPackageMetas = new List<QuestionnaireSyncPackageMeta>
            {
                CreateQuestionnaireSyncPackageMetaInformation(questionnaireId, questionnaireVersion:1, sortIndex:1, itemType: SyncItemType.Questionnaire),
                CreateQuestionnaireSyncPackageMetaInformation(questionnaireId, questionnaireVersion:1, sortIndex:2, itemType: SyncItemType.QuestionnaireAssembly),
                CreateQuestionnaireSyncPackageMetaInformation(questionnaireId, questionnaireVersion:1, sortIndex:3, itemType: SyncItemType.DeleteQuestionnaire)
            };
            
            lastSyncedPackageId = questionnaireSyncPackageMetas[0].PackageId;


            var writer = Stub.ReadSideRepository<QuestionnaireSyncPackageMeta>();
            foreach (var package in questionnaireSyncPackageMetas)
            {
                writer.Store(package, package.PackageId);
            }

            syncManager = CreateSyncManager(devices: devices, questionnairesReader: writer);
        };

        Because of = () =>
            result = syncManager.GetQuestionnairePackageIdsWithOrder(userId, deviceId, lastSyncedPackageId);

        It should_return_not_null_result = () =>
            result.ShouldNotBeNull();

        It should_return_list_with_1_package_id = () =>
            result.SyncPackagesMeta.Count().ShouldEqual(1);

        It should_return_list_with_package_id_specified = () =>
            result.SyncPackagesMeta.Select(x => x.Id).ShouldContainOnly("22222222222222222222222222222222_1$3");

        It should_return_list_with_ordered_by_index_items = () =>
            result.SyncPackagesMeta.Select(x => x.SortIndex).ShouldEqual(new long[] { 3 });

        private static SyncManager syncManager;
        private static SyncItemsMetaContainer result;

        private static string androidId = "Android";
        private static Guid deviceId = androidId.ToGuid();
        private static TabletDocument tabletDocument;
        private static IReadSideRepositoryReader<TabletDocument> devices;

        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static string lastSyncedPackageId = null;

        private static Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static List<QuestionnaireSyncPackageMeta> questionnaireSyncPackageMetas;
    }
}