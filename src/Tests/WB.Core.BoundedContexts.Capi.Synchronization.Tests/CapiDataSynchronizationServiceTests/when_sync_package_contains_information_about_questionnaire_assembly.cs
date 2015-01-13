using System;
using Machine.Specifications;
using Main.Core.Utility;
using Moq;
using WB.Core.BoundedContexts.Capi.Synchronization.ChangeLog;
using WB.Core.BoundedContexts.Capi.Synchronization.Implementation.Services;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests.CapiDataSynchronizationServiceTests
{
    internal class when_sync_package_contains_information_about_questionnaire_assembly : CapiDataSynchronizationServiceTestContext
    {
        Establish context = () =>
        {
            var meta = new QuestionnaireAssemblyMetadata(questionnaireId, version);

            var syncItem = new SyncItem
            {
                Id = questionnaireId.Combine(version),
                ItemType = SyncItemType.QuestionnaireAssembly,
                IsCompressed = true,
                Content = GetItemAsContent(assemblyAsBase64),
                MetaInfo = GetItemAsContent(meta)
            };

            var item = new SynchronizationDelta(syncItem.Id, syncItem.Content, DateTime.Now, userId, syncItem.IsCompressed, 
                syncItem.ItemType, syncItem.MetaInfo);


            received = new SyncItem
            {
                Id = item.PublicKey,
                IsCompressed = item.IsCompressed,
                ItemType = item.ItemType,
                Content = item.Content,
                MetaInfo = item.MetaInfo
            };

            var jsonUtils = new NewtonJsonUtils();
            var compressor = new GZipJsonCompressor(jsonUtils);
            
            changeLogManipulator = new Mock<IChangeLogManipulator>();

            questionnareAssemblyFileAccessor = new Mock<IQuestionnaireAssemblyFileAccessor>();

            capiDataSynchronizationService = CreateCapiDataSynchronizationService(changeLogManipulator.Object, stringCompressor : compressor,
                jsonUtils : jsonUtils, questionnareAssemblyFileAccessor: questionnareAssemblyFileAccessor.Object);
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
