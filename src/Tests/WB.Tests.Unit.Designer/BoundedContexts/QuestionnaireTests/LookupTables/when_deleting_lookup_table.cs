using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.LookupTables;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.LookupTables
{
    internal class when_deleting_lookup_table : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);
            questionnaire.AddLookupTable(Create.Command.AddLookupTable(questionnaireId, lookupTableId, responsibleId));

            deleteLookupTable = Create.Command.DeleteLookupTable(questionnaireId, lookupTableId, responsibleId);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => questionnaire.DeleteLookupTable(deleteLookupTable);

        It should_raise_LookupTableDeleted_event_with_EntityId_specified = () =>
            eventContext.GetSingleEvent<LookupTableDeleted>().LookupTableId.ShouldEqual(lookupTableId);

        It should_raise_LookupTableDeleted_event_with_ResponsibleId_specified = () =>
            eventContext.GetSingleEvent<LookupTableDeleted>().ResponsibleId.ShouldEqual(responsibleId);

        private static DeleteLookupTable deleteLookupTable;
        private static Questionnaire questionnaire;
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static EventContext eventContext;
    }
}
