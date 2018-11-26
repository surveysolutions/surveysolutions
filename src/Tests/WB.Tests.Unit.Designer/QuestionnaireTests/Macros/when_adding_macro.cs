using System;
using FluentAssertions;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Macros
{
    internal class when_adding_macro : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);

            addMacro = Create.Command.AddMacro(questionnaireId, macroId, responsibleId);
            BecauseOf();
        }


        private void BecauseOf() => questionnaire.AddMacro(addMacro);

        [NUnit.Framework.Test] public void should_contains_Macro_with_EntityId_specified () =>
            questionnaire.QuestionnaireDocument.Macros.ContainsKey(macroId).Should().BeTrue();

        private static AddMacro addMacro;
        private static Questionnaire questionnaire;
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid macroId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}
