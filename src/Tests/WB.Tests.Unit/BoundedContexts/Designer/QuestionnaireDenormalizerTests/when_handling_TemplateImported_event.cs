using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using It = Machine.Specifications.It;
using it = Moq.It;
using Main.DenormalizerStorage;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireDenormalizerTests
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

            @event = Create.TemplateImportedEvent(questionnaireId: questionnaireDocument.PublicKey.FormatGuid());
            @event.Payload.Source.SharedPersons.Add(Guid.NewGuid());

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_list_of_shared_persons_contains_persons_from_replaced_questionnaire_only = () =>
           documentStorage.GetById(questionnaireDocument.PublicKey).SharedPersons.ShouldContainOnly(shredPersonWithBefore);

        private static QuestionnaireDenormalizer denormalizer;
        private static QuestionnaireDocument questionnaireDocument;
        private static IPublishedEvent<designer::Main.Core.Events.Questionnaire.TemplateImported> @event;
        private static Guid shredPersonWithBefore = Guid.Parse("11111111111111111111111111111111");
        private static InMemoryReadSideRepositoryAccessor<QuestionnaireDocument> documentStorage;
    }
}
