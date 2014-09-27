using System;
using Machine.Specifications;
using Main.Core.Utility;
using Moq;
using WB.Core.BoundedContexts.Capi.Synchronization.ChangeLog;
using WB.Core.BoundedContexts.Capi.Synchronization.Implementation.Services;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests.CapiDataSynchronizationServiceTests
{
    internal class when_sync_package_contains_information_about_questionnaire_assembly_with_broken_metadata : CapiDataSynchronizationServiceTestContext
    {
        Establish context = () =>
        {
            var meta = new QuestionnaireAssemblyMetadata(questionnaireId, version);

            syncItem = new SyncItem
            {
                Id = questionnaireId.Combine(version),
                ItemType = SyncItemType.QuestionnaireAssembly,
                IsCompressed = false,
                Content = "some_content",
                MetaInfo = "dummy meta"
            };

            var jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserrialize<QuestionnaireAssemblyMetadata>(Moq.It.IsAny<string>())).Throws<NullReferenceException>();
            
            changeLogManipulator = new Mock<IChangeLogManipulator>();

            questionnareAssemblyFileAccessor = new Mock<IQuestionnaireAssemblyFileAccessor>();

            capiDataSynchronizationService = CreateCapiDataSynchronizationService(changeLogManipulator.Object,
                jsonUtils : jsonUtilsMock.Object, questionnareAssemblyFileAccessor: questionnareAssemblyFileAccessor.Object);
        };

        Because of = () => exception = Catch.Exception(() => capiDataSynchronizationService.SavePulledItem(syncItem));

        It should_call_StoreAssembly_zero_time =
            () =>
                questionnareAssemblyFileAccessor.Verify(
                    x => x.StoreAssembly(questionnaireId, version, assemblyAsBase64),
                    Times.Never);

        It should_throw_ArgumentException = () =>
            exception.ShouldBeOfType<ArgumentException>();

        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long version = 3;
        private static CapiDataSynchronizationService capiDataSynchronizationService;
        private static SyncItem syncItem;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
        private static Exception exception;
        private static string assemblyAsBase64 = "some_content";
        private static Mock<IQuestionnaireAssemblyFileAccessor> questionnareAssemblyFileAccessor;
    }
}
