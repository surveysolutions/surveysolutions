using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.LookupTables;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_LookupTableDeleted_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            evnt = Create.Event.LookupTableDeleted(questionnaireId, entityId);

            questionnaire = Create.QuestionnaireDocument(questionnaireId);

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaire);
            denormalizer.DeleteLookupTable(evnt.Payload);
        };

        Because of = () => denormalizer.DeleteLookupTable(evnt.Payload);

        It should_delete_lookup_table = () =>
            questionnaire.LookupTables.Count.ShouldEqual(0);

        private static Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Questionnaire denormalizer;
        private static QuestionnaireDocument questionnaire;
        private static IPublishedEvent<LookupTableDeleted> evnt;
    }
}