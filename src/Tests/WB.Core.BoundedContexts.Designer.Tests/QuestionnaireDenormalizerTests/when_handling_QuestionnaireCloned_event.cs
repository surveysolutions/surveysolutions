using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDenormalizerTests
{
    internal class when_handling_QuestionnaireCloned_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            documentStorage = new Mock<IReadSideRepositoryWriter<QuestionnaireDocument>>();

            var initialDocument = CreateQuestionnaireDocument();

            questionnaireClonedEvent = CreateQuestionnaireClonedEvent(initialDocument);

            sequenceUpgradeResult = CreateQuestionnaireDocument();
            var translatedDocument = CreateQuestionnaireDocument();

            var upgrader = Mock.Of<IQuestionnaireDocumentUpgrader>(u
                => u.TranslatePropagatePropertiesToRosterProperties(initialDocument) == translatedDocument
                && u.CleanExpressionCaches(translatedDocument) == sequenceUpgradeResult);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage.Object, upgrader: upgrader);
        };

        Because of = () =>
            denormalizer.Handle(questionnaireClonedEvent);

        It should_pass_result_of_sequence_of_calls_to_QuestionnaireDocumentUpgrader_to_document_storages_Store_method = () =>
            documentStorage.Verify(s => s.Store(
                Moq.It.Is<QuestionnaireDocument>(d => d == sequenceUpgradeResult),
                Moq.It.Is<string>(g => g == sequenceUpgradeResult.PublicKey.FormatGuid())));

        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<QuestionnaireCloned> questionnaireClonedEvent;
        private static QuestionnaireDocument sequenceUpgradeResult;
        private static Mock<IReadSideRepositoryWriter<QuestionnaireDocument>> documentStorage;
    }
}