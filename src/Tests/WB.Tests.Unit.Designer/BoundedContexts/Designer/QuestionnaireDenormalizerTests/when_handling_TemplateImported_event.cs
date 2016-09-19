using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    extern alias designer;

    internal class when_handling_TemplateImported_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            questionnaireDocument = CreateQuestionnaireDocument();
            questionnaireDocument.SharedPersons.Add(shredPersonWithBefore);

            documentStorage = new InMemoryReadSideRepositoryAccessor<QuestionnaireDocument>();
            documentStorage.Store(questionnaireDocument, questionnaireDocument.PublicKey);

            @event = Create.Event.TemplateImportedEvent(questionnaireId: questionnaireDocument.PublicKey.FormatGuid());
            @event.Payload.Source.SharedPersons.Add(Guid.NewGuid());

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaireDocument);
        };

        Because of = () =>
            denormalizer.ImportTemplate(@event.Payload);

        It should_list_of_shared_persons_contains_persons_from_replaced_questionnaire_only = () =>
           documentStorage.GetById(questionnaireDocument.PublicKey).SharedPersons.ShouldContainOnly(shredPersonWithBefore);

        private static Questionnaire denormalizer;
        private static QuestionnaireDocument questionnaireDocument;
        private static IPublishedEvent<designer::Main.Core.Events.Questionnaire.TemplateImported> @event;
        private static Guid shredPersonWithBefore = Guid.Parse("11111111111111111111111111111111");
        private static InMemoryReadSideRepositoryAccessor<QuestionnaireDocument> documentStorage;
    }
}
