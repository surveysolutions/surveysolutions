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

            eventContext = new EventContext();
        };

        Because of = () =>
                questionnaire.AddVariableAndMoveIfNeeded(
                    new AddVariable(questionnaire.EventSourceId, entityId, new VariableData(variableType, variableName, variableExpression), responsibleId, chapterId, index));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_VariableAdded_event = () =>
            eventContext.ShouldContainEvent<VariableAdded>();

        It should_raise_VariableAdded_event_with_EntityId_specified = () =>
            eventContext.GetSingleEvent<VariableAdded>().EntityId.ShouldEqual(entityId);

        It should_raise_VariableAdded_event_with_ParentId_specified = () =>
            eventContext.GetSingleEvent<VariableAdded>().ParentId.ShouldEqual(chapterId);

        It should_raise_VariableAdded_event_with_name_specified = () =>
            eventContext.GetSingleEvent<VariableAdded>().VariableData.Name.ShouldEqual(variableName);

        It should_raise_VariableAdded_event_with_expression_specified = () =>
            eventContext.GetSingleEvent<VariableAdded>().VariableData.Expression.ShouldEqual(variableExpression);

        It should_raise_VariableAdded_event_with_type_specified = () =>
            eventContext.GetSingleEvent<VariableAdded>().VariableData.Type.ShouldEqual(variableType);

        It should_raise_QuestionnaireItemMoved_event = () =>
            eventContext.ShouldContainEvent<QuestionnaireItemMoved>();

        It should_raise_QuestionnaireItemMoved_event_with_GroupKey_specified = () =>
            eventContext.GetSingleEvent<QuestionnaireItemMoved>().GroupKey.ShouldEqual(chapterId);

        It should_raise_QuestionnaireItemMoved_event_with_PublicKey_specified = () =>
            eventContext.GetSingleEvent<QuestionnaireItemMoved>().PublicKey.ShouldEqual(entityId);

        It should_raise_QuestionnaireItemMoved_event_with_TargetIndex_specified = () =>
            eventContext.GetSingleEvent<QuestionnaireItemMoved>().TargetIndex.ShouldEqual(index);

        It should_raise_QuestionnaireItemMoved_event_with_ResponsibleId_specified = () =>
           eventContext.GetSingleEvent<QuestionnaireItemMoved>().ResponsibleId.ShouldEqual(responsibleId);

        private static EventContext eventContext;
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