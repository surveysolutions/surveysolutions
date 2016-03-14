using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.LookupTables;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.LookupTables
{
    internal class when_updating_lookup_table : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);
            questionnaire.AddLookupTable(Create.Command.AddLookupTable(questionnaireId, lookupTableId, responsibleId));

            updateLookupTable = Create.Command.UpdateLookupTable(questionnaireId, lookupTableId, responsibleId);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => questionnaire.UpdateLookupTable(updateLookupTable);

        It should_raise_LookupTableUpdated_event_with_EntityId_specified = () =>
            eventContext.GetSingleEvent<LookupTableUpdated>().LookupTableId.ShouldEqual(lookupTableId);

        It should_raise_LookupTableUpdated_event_with_ResponsibleId_specified = () =>
            eventContext.GetSingleEvent<LookupTableUpdated>().ResponsibleId.ShouldEqual(responsibleId);

        private static UpdateLookupTable updateLookupTable;
        private static Questionnaire questionnaire;
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static EventContext eventContext;
    }
}
