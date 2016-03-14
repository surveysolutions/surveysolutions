using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Macros;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_deleting_macro_with_premission_to_edit : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: ownerId);
            questionnaire.AddMacro(Create.Command.AddMacro(questionnaireId, macroId, ownerId));
            questionnaire.AddSharedPerson(sharedPersonId, "email@email.com", ShareType.Edit, ownerId);

            deleteMacro = Create.Command.DeleteMacro(questionnaireId, macroId, sharedPersonId);

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
            eventContext.GetSingleEvent<MacroDeleted>().ResponsibleId.ShouldEqual(sharedPersonId);

        private static DeleteMacro deleteMacro;
        private static Questionnaire questionnaire;
        private static readonly Guid ownerId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid sharedPersonId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid macroId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static EventContext eventContext;
    }
}