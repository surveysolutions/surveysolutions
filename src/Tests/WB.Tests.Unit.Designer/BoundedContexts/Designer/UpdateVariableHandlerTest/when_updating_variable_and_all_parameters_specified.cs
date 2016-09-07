using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateVariableHandlerTest
{
    internal class when_updating_variable_and_all_parameters_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddVariable(Create.Event.VariableAdded(entityId : entityId, parentId : chapterId ));

            eventContext = new EventContext();

            command = Create.Command.UpdateVariable(
                questionnaire.EventSourceId,
                entityId: entityId,
                type: variableType,
                name: variableName,
                expression: variableExpression,
                userId: responsibleId
                );
        };

        Because of = () =>            
                questionnaire.UpdateVariable(command);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_VariableUpdated_event = () =>
            eventContext.ShouldContainEvent<VariableUpdated>();

        It should_raise_VariableUpdated_event_with_EntityId_specified = () =>
            eventContext.GetSingleEvent<VariableUpdated>().EntityId.ShouldEqual(entityId);

        It should_raise_VariableUpdated_event_with_name_specified = () =>
            eventContext.GetSingleEvent<VariableUpdated>().VariableData.Name.ShouldEqual(variableName);

        It should_raise_VariableUpdated_event_with_Type_specified = () =>
            eventContext.GetSingleEvent<VariableUpdated>().VariableData.Type.ShouldEqual(variableType);

        It should_raise_VariableUpdated_event_with_Expression_specified = () =>
            eventContext.GetSingleEvent<VariableUpdated>().VariableData.Expression.ShouldEqual(variableExpression);


        private static UpdateVariable command;
        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "name";
        private static string variableExpression = "expression";
        private static VariableType variableType = VariableType.Double;
    }
}