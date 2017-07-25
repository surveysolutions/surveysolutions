using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;

using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AddVariableHandlerTests
{
    internal class when_adding_variable_to_chapter : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            BecauseOf();
        }

        private void BecauseOf() =>
                questionnaire.AddVariableAndMoveIfNeeded(
                    new AddVariable(questionnaire.Id, entityId, new VariableData(variableType, variableName, variableExpression, description), responsibleId, chapterId, index));


        [NUnit.Framework.Test] public void should_contains_Variable_with_EntityId_specified () =>
            questionnaire.QuestionnaireDocument.Find<Variable>(entityId).PublicKey.ShouldEqual(entityId);

        [NUnit.Framework.Test] public void should_contains_Variable_with_ParentId_specified () =>
            questionnaire.QuestionnaireDocument.Find<Variable>(entityId).GetParent().PublicKey.ShouldEqual(chapterId);

        [NUnit.Framework.Test] public void should_contains_Variable_with_name_specified () =>
            questionnaire.QuestionnaireDocument.Find<Variable>(entityId).Name.ShouldEqual(variableName);

        [NUnit.Framework.Test] public void should_contains_Variable_with_expression_specified () =>
            questionnaire.QuestionnaireDocument.Find<Variable>(entityId).Expression.ShouldEqual(variableExpression);

        [NUnit.Framework.Test] public void should_contains_Variable_with_type_specified () =>
            questionnaire.QuestionnaireDocument.Find<Variable>(entityId).Type.ShouldEqual(variableType);

        [NUnit.Framework.Test] public void should_change_variable_description () =>
          questionnaire.QuestionnaireDocument.Find<Variable>(entityId).Label.ShouldEqual(description);

        private static Questionnaire questionnaire;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "name";
        private static string variableExpression = "expression";
        private static string description = "description";
        private static VariableType variableType = VariableType.Double;
        private static int index = 5;
    }
}