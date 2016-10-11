using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateVariableHandlerTest
{
    internal class when_updating_variable_and_all_parameters_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddVariable(entityId : entityId, parentId : chapterId ,responsibleId:responsibleId);

            command = Create.Command.UpdateVariable(
                questionnaire.Id,
                entityId: entityId,
                type: variableType,
                name: variableName,
                expression: variableExpression,
                userId: responsibleId
                );
        };

        Because of = () =>            
                questionnaire.UpdateVariable(command);


        It should_contains_variable = () =>
            questionnaire.QuestionnaireDocument.Find<IVariable>(entityId);

        It should_contains_variable_with_EntityId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IVariable>(entityId).PublicKey.ShouldEqual(entityId);

        It should_contains_variable_with_name_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IVariable>(entityId).Name.ShouldEqual(variableName);

        It should_contains_variable_with_Type_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IVariable>(entityId).Type.ShouldEqual(variableType);

        It should_contains_variable_with_Expression_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IVariable>(entityId).Expression.ShouldEqual(variableExpression);


        private static UpdateVariable command;
        private static Questionnaire questionnaire;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "name";
        private static string variableExpression = "expression";
        private static VariableType variableType = VariableType.Double;
    }
}