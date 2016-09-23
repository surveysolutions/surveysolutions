using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Macros
{
    internal class when_updating_macro_with_premission_to_edit : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: ownerId);
            questionnaire.AddMacro(Create.Command.AddMacro(questionnaireId, macroId, ownerId));
            questionnaire.AddSharedPerson(sharedPersonId, "email@email.com", ShareType.Edit, ownerId);

            updateMacro = Create.Command.UpdateMacro(questionnaireId, macroId, name, content, description, sharedPersonId);
        };


        Because of = () => questionnaire.UpdateMacro(updateMacro);

        It should_contains_Macro_with_EntityId_specified = () =>
            questionnaire.QuestionnaireDocument.Macros.ShouldContain(t => t.Key == macroId);

        It should_contains_Macro_with_Name_specified = () =>
            questionnaire.QuestionnaireDocument.Macros.Single(t => t.Key == macroId).Value.Name.ShouldEqual(name);

        It should_contains_Macro_with_Content_specified = () =>
            questionnaire.QuestionnaireDocument.Macros.Single(t => t.Key == macroId).Value.Content.ShouldEqual(content);

        It should_contains_Macro_with_Description_specified = () =>
            questionnaire.QuestionnaireDocument.Macros.Single(t => t.Key == macroId).Value.Description.ShouldEqual(description);

        private static UpdateMacro updateMacro;
        private static Questionnaire questionnaire;
        private static readonly string name = "macros";
        private static readonly string content = "macros content";
        private static readonly string description = "macros description";
        private static readonly Guid ownerId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid sharedPersonId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid macroId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}