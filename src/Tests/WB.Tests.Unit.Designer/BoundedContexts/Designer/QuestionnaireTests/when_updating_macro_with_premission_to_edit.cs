using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Macros;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_updating_macro_with_premission_to_edit : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: ownerId);
            questionnaire.AddMacro(Create.Command.AddMacro(questionnaireId, macroId, ownerId));
            questionnaire.AddSharedPerson(sharedPersonId, "email@email.com", ShareType.Edit, ownerId);

            updateMacro = Create.Command.UpdateMacro(questionnaireId, macroId, name, content, description, sharedPersonId);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => questionnaire.UpdateMacro(updateMacro);

        It should_raise_MacroUpdated_event_with_EntityId_specified = () =>
            eventContext.GetSingleEvent<MacroUpdated>().MacroId.ShouldEqual(macroId);

        It should_raise_MacroUpdated_event_with_ResponsibleId_specified = () =>
            eventContext.GetSingleEvent<MacroUpdated>().ResponsibleId.ShouldEqual(sharedPersonId);

        It should_raise_MacroUpdated_event_with_Name_specified = () =>
            eventContext.GetSingleEvent<MacroUpdated>().Name.ShouldEqual(name);

        It should_raise_MacroUpdated_event_with_Content_specified = () =>
            eventContext.GetSingleEvent<MacroUpdated>().Content.ShouldEqual(content);

        It should_raise_MacroUpdated_event_with_Description_specified = () =>
            eventContext.GetSingleEvent<MacroUpdated>().Description.ShouldEqual(description);

        private static UpdateMacro updateMacro;
        private static Questionnaire questionnaire;
        private static readonly string name = "macros";
        private static readonly string content = "macros content";
        private static readonly string description = "macros description";
        private static readonly Guid ownerId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid sharedPersonId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid macroId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static EventContext eventContext;
    }
}