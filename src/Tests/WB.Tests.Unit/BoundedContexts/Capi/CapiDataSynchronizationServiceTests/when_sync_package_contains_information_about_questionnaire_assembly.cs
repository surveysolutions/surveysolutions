using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Rest;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.SurveySolutions.Services;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Capi.CapiDataSynchronizationServiceTests
{
    internal class when_sync_package_contains_information_about_questionnaire_assembly : CapiDataSynchronizationServiceTestContext
    {
        Establish context = () =>
        {
            var meta = new QuestionnaireAssemblyMetadata(questionnaireId, version);

            var jsonUtils = new NewtonJsonUtils();
            var compressor = new JsonCompressor(jsonUtils);

            var syncItem = new SyncItem
            {
                RootId = questionnaireId.Combine(version),
                ItemType = SyncItemType.QuestionnaireAssembly,
                IsCompressed = true,
                Content = compressor.CompressString(GetItemAsContent(assemblyAsBase64)),
                MetaInfo = compressor.CompressString(GetItemAsContent(meta))
            };

            var item = new SynchronizationDeltaMetaInformation(syncItem.RootId, DateTime.Now, userId, "q", 1,
                syncItem.Content.Length, 0);

            received = new SyncItem
            {
                RootId = syncItem.RootId,
                IsCompressed = syncItem.IsCompressed,
                ItemType = syncItem.ItemType,
                Content = syncItem.Content,
                MetaInfo = syncItem.MetaInfo
            };

            changeLogManipulator = new Mock<IChangeLogManipulator>();

            questionnareAssemblyFileAccessor = new Mock<IQuestionnaireAssemblyFileAccessor>();

            capiDataSynchronizationService = CreateCapiDataSynchronizationService(changeLogManipulator.Object, stringCompressor: compressor,
                jsonUtils: jsonUtils, questionnareAssemblyFileAccessor: questionnareAssemblyFileAccessor.Object);
        };

        Because of = () => capiDataSynchronizationService.SavePulledItem(received);

        It should_call_StoreAssembly_once =
            () =>
                questionnareAssemblyFileAccessor.Verify(
                    x => x.StoreAssembly(questionnaireId, version, assemblyAsBase64),
                    Times.Once);

        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long version = 3;
        private static CapiDataSynchronizationService capiDataSynchronizationService;
        private static SyncItem received;
        private static Mock<IChangeLogManipulator> changeLogManipulator;

        private static Guid userId = Guid.Parse("11111111111111111111111111111113");

        private static string assemblyAsBase64 = "some_content";
        private static Mock<IQuestionnaireAssemblyFileAccessor> questionnareAssemblyFileAccessor;
    }

}
