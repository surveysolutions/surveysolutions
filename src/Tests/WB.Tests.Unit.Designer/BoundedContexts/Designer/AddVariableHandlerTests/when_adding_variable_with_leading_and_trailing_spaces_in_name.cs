using System;
using FluentAssertions;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AddVariableHandlerTests
{
    internal class when_adding_variable_with_leading_and_trailing_spaces_in_name : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context()
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId: responsibleId);
            BecauseOf();
        }

        private void BecauseOf() =>
            questionnaire.AddVariableAndMoveIfNeeded(
                new AddVariable(questionnaire.Id, entityId,
                    new VariableData(VariableType.String, "  myVariable  ", "expression", null, false),
                    responsibleId, chapterId));

        [NUnit.Framework.Test] public void should_store_variable_name_without_leading_and_trailing_spaces() =>
            questionnaire.QuestionnaireDocument.Find<Variable>(entityId).Name.Should().Be("myVariable");

        private static Questionnaire questionnaire;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111113");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}
