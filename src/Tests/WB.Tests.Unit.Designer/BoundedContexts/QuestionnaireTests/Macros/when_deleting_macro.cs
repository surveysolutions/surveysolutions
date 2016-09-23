using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Macros
{
    internal class when_deleting_macro : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);
            questionnaire.AddMacro(Create.Command.AddMacro(questionnaireId, macroId, responsibleId));

            deleteMacro = Create.Command.DeleteMacro(questionnaireId, macroId, responsibleId);
        };


        Because of = () => questionnaire.DeleteMacro(deleteMacro);

        It should_doesnt_contain_Macro_with_EntityId_specified = () =>
            questionnaire.QuestionnaireDocument.Macros.ShouldNotContain(t => t.Key == macroId);


        private static DeleteMacro deleteMacro;
        private static Questionnaire questionnaire;
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid macroId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}