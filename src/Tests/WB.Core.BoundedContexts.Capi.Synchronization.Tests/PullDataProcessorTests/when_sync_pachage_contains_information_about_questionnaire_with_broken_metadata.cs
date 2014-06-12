using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi.Synchronization.Synchronization.Pull;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests.PullDataProcessorTests
{
    internal class when_sync_pachage_contains_information_about_questionnaire_with_broken_metadata : PullDataProcessorTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument = new QuestionnaireDocument()
            {
                PublicKey = Guid.NewGuid()
            };

            syncItem = new SyncItem() { ItemType = SyncItemType.Template, IsCompressed = true, Content = "some content", MetaInfo = "some metadata" };

            var jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserrialize<QuestionnaireDocument>(syncItem.Content)).Returns(questionnaireDocument);
            jsonUtilsMock.Setup(x => x.Deserrialize<QuestionnaireMetadata>(syncItem.MetaInfo)).Throws<NullReferenceException>();

            commandService = new Mock<ICommandService>();

            plainQuestionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();

            pullDataProcessor = CreatePullDataProcessor(commandService.Object, jsonUtilsMock.Object, null,
                plainQuestionnaireRepositoryMock.Object);
        };

        Because of = () => exception = Catch.Exception(() =>pullDataProcessor.Process(syncItem));

        It should_call_RegisterPlainQuestionnaire_zero_times =
            () =>
                commandService.Verify(
                    x =>
                        x.Execute(
                            Moq.It.Is<RegisterPlainQuestionnaire>(
                                param =>
                                    param.QuestionnaireId == questionnaireDocument.PublicKey && param.Version == Moq.It.IsAny<long>()), null),
                    Times.Never);

        It should_store_questionnaire_in_plaine_storage_zero_times =
            () =>
                plainQuestionnaireRepositoryMock.Verify(
                    x => x.StoreQuestionnaire(questionnaireDocument.PublicKey, Moq.It.IsAny<long>(), questionnaireDocument),
                    Times.Never);

        It should_throw_ArgumentException = () =>
            exception.ShouldBeOfType<ArgumentException>();

        private static PullDataProcessor pullDataProcessor;
        private static SyncItem syncItem;
        private static QuestionnaireDocument questionnaireDocument;
        private static Mock<ICommandService> commandService;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
        private static Exception exception;
    }
}
