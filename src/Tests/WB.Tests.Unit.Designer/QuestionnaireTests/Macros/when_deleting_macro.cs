using System;
using FluentAssertions;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Macros
{
    internal class when_deleting_macro : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);
            questionnaire.AddMacro(Create.Command.AddMacro(questionnaireId, macroId, responsibleId));

            deleteMacro = Create.Command.DeleteMacro(questionnaireId, macroId, responsibleId);
            BecauseOf();
        }


        private void BecauseOf() => questionnaire.DeleteMacro(deleteMacro);

        [NUnit.Framework.Test] public void should_doesnt_contain_Macro_with_EntityId_specified () =>
            questionnaire.QuestionnaireDocument.Macros.ContainsKey(macroId).Should().BeFalse();


        private static DeleteMacro deleteMacro;
        private static Questionnaire questionnaire;
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid macroId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}
