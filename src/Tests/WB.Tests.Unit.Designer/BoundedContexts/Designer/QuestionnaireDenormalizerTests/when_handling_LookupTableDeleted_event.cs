using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_LookupTableDeleted_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            evnt = Create.Event.LookupTableDeleted(questionnaireId, entityId);

            questionnaire = Create.QuestionnaireDocument(questionnaireId);

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(storage => storage.GetById(Moq.It.IsAny<string>()) == questionnaire);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
            denormalizer.Handle(evnt);
        };

        Because of = () => denormalizer.Handle(evnt);

        It should_delete_lookup_table = () =>
            questionnaire.LookupTables.Count.ShouldEqual(0);

        private static Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static QuestionnaireDenormalizer denormalizer;
        private static QuestionnaireDocument questionnaire;
        private static IPublishedEvent<LookupTableDeleted> evnt;
    }
}