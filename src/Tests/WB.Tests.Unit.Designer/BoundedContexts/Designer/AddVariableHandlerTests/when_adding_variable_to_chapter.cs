using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AddVariableHandlerTests
{
    internal class when_adding_variable_to_chapter : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
        };

        Because of = () =>
                questionnaire.AddVariableAndMoveIfNeeded(
                    new AddVariable(questionnaire.EventSourceId, entityId, new VariableData(variableType, variableName, variableExpression), responsibleId, chapterId, index));


        It should_contains_Variable_with_EntityId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<Variable>(entityId).PublicKey.ShouldEqual(entityId);

        It should_contains_Variable_with_ParentId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<Variable>(entityId).GetParent().PublicKey.ShouldEqual(chapterId);

        It should_contains_Variable_with_name_specified = () =>
            questionnaire.QuestionnaireDocument.Find<Variable>(entityId).Name.ShouldEqual(variableName);

        It should_contains_Variable_with_expression_specified = () =>
            questionnaire.QuestionnaireDocument.Find<Variable>(entityId).Expression.ShouldEqual(variableExpression);

        It should_contains_Variable_with_type_specified = () =>
            questionnaire.QuestionnaireDocument.Find<Variable>(entityId).Type.ShouldEqual(variableType);


        private static Questionnaire questionnaire;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "name";
        private static string variableExpression = "expression";
        private static VariableType variableType = VariableType.Double;
        private static int index = 5;
    }
}