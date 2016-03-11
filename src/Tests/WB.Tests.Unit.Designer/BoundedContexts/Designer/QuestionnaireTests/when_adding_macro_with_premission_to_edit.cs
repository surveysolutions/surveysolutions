using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Macros;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_adding_macro_with_premission_to_edit : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: ownerId);
            addMacro = Create.Command.AddMacro(questionnaireId, macroId, sharedPersonId);
            questionnaire.AddSharedPerson(sharedPersonId, "email@email.com", ShareType.Edit, ownerId);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => questionnaire.AddMacro(addMacro);

        It should_raise_MacroAdded_event_with_EntityId_specified = () =>
            eventContext.GetSingleEvent<MacroAdded>().MacroId.ShouldEqual(macroId);

        It should_raise_MacroAdded_event_with_ResponsibleId_specified = () =>
            eventContext.GetSingleEvent<MacroAdded>().ResponsibleId.ShouldEqual(sharedPersonId);

        private static AddMacro addMacro;
        private static Questionnaire questionnaire;
        private static readonly Guid ownerId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid sharedPersonId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid macroId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static EventContext eventContext;
    }
}