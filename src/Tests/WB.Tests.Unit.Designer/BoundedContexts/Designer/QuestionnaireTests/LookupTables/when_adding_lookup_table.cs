using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Macros;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests.LookupTables
{
    internal class when_adding_lookup_table : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);

            addLookupTable = Create.Command.AddLookupTable(questionnaireId, lookupTableId, responsibleId);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => questionnaire.AddLookupTable(addLookupTable);

        It should_raise_LookupTableAdded_event_with_EntityId_specified = () =>
            eventContext.GetSingleEvent<LookupTableAdded>().LookupTableId.ShouldEqual(lookupTableId);

        It should_raise_LookupTableAdded_event_with_ResponsibleId_specified = () =>
            eventContext.GetSingleEvent<LookupTableAdded>().ResponsibleId.ShouldEqual(responsibleId);

        private static AddLookupTable addLookupTable;
        private static Questionnaire questionnaire;
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static EventContext eventContext;
    }
}
