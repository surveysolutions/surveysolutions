using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Macros;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_deleting_macro : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);
            questionnaire.AddMacro(Create.Command.AddMacro(questionnaireId, macroId, responsibleId));

            deleteMacro = Create.Command.DeleteMacro(questionnaireId, macroId, responsibleId);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => questionnaire.DeleteMacro(deleteMacro);

        It should_raise_MacroDeleted_event_with_EntityId_specified = () =>
            eventContext.GetSingleEvent<MacroDeleted>().MacroId.ShouldEqual(macroId);

        It should_raise_MacroDeleted_event_with_ResponsibleId_specified = () =>
            eventContext.GetSingleEvent<MacroDeleted>().ResponsibleId.ShouldEqual(responsibleId);

        private static DeleteMacro deleteMacro;
        private static Questionnaire questionnaire;
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid macroId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static EventContext eventContext;
    }
}