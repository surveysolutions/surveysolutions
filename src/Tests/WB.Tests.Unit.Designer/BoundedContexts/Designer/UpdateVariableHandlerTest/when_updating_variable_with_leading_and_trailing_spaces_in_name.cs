using System;
using FluentAssertions;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateVariableHandlerTest
{
    internal class when_updating_variable_with_leading_and_trailing_spaces_in_name : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context()
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId: responsibleId);
            questionnaire.AddVariable(entityId: entityId, parentId: chapterId, responsibleId: responsibleId);

            command = Create.Command.UpdateVariable(
                questionnaire.Id,
                entityId: entityId,
                type: VariableType.String,
                name: "  myVariable  ",
                expression: "expression",
                userId: responsibleId
            );
            BecauseOf();
        }

        private void BecauseOf() =>
            questionnaire.UpdateVariable(command);

        [NUnit.Framework.Test] public void should_store_variable_name_without_leading_and_trailing_spaces() =>
            questionnaire.QuestionnaireDocument.Find<IVariable>(entityId).Name.Should().Be("myVariable");

        private static UpdateVariable command;
        private static Questionnaire questionnaire;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111113");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}
