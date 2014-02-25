using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Moq;
using Moq.Language.Flow;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDenormalizerTests
{
    internal class when_handling_TemplateImported_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            documentStorage = new Mock<IReadSideRepositoryWriter<QuestionnaireDocument>>();

            var initialDocument = CreateQuestionnaireDocument();

            templateImportedEvent = CreateTemplateImportedEvent(initialDocument);

            sequenceUpgradeResult = CreateQuestionnaireDocument();
            var translatedDocument = CreateQuestionnaireDocument();

            var upgrader = Mock.Of<IQuestionnaireDocumentUpgrader>(u
                => u.TranslatePropagatePropertiesToRosterProperties(initialDocument) == translatedDocument
                && u.CleanExpressionCaches(translatedDocument) == sequenceUpgradeResult);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage.Object, upgrader: upgrader);
        };

        Because of = () =>
            denormalizer.Handle(templateImportedEvent);

        It should_pass_result_of_sequence_of_calls_to_QuestionnaireDocumentUpgrader_to_document_storages_Store_method = () =>
            documentStorage.Verify(s => s.Store(
                it.Is<QuestionnaireDocument>(d => d == sequenceUpgradeResult),
                it.Is<string>(g => g == sequenceUpgradeResult.PublicKey.FormatGuid())));

        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<TemplateImported> templateImportedEvent;
        private static QuestionnaireDocument sequenceUpgradeResult;
        private static Mock<IReadSideRepositoryWriter<QuestionnaireDocument>> documentStorage;
    }
}
