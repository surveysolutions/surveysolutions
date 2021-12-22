using System;
using FluentAssertions;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;

using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateVariableHandlerTest
{
    internal class when_updating_variable_and_all_parameters_specified : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddVariable(entityId : entityId, parentId : chapterId ,responsibleId:responsibleId);

            command = Create.Command.UpdateVariable(
                questionnaire.Id,
                entityId: entityId,
                type: variableType,
                name: variableName,
                expression: variableExpression,
                userId: responsibleId,
                doNotExport: true
                );
            BecauseOf();
        }

        private void BecauseOf() =>            
                questionnaire.UpdateVariable(command);


        [NUnit.Framework.Test] public void should_contains_variable () =>
            questionnaire.QuestionnaireDocument.Find<IVariable>(entityId).Should().NotBeNull();

        [NUnit.Framework.Test] public void should_contains_variable_with_EntityId_specified () =>
            questionnaire.QuestionnaireDocument.Find<IVariable>(entityId).PublicKey.Should().Be(entityId);

        [NUnit.Framework.Test] public void should_contains_variable_with_name_specified () =>
            questionnaire.QuestionnaireDocument.Find<IVariable>(entityId).Name.Should().Be(variableName);

        [NUnit.Framework.Test] public void should_contains_variable_with_Type_specified () =>
            questionnaire.QuestionnaireDocument.Find<IVariable>(entityId).Type.Should().Be(variableType);

        [NUnit.Framework.Test] public void should_contains_variable_with_Expression_specified () =>
            questionnaire.QuestionnaireDocument.Find<IVariable>(entityId).Expression.Should().Be(variableExpression);

        [NUnit.Framework.Test]
        public void should_contains_variable_with_DoNotExport_specified() =>
            questionnaire.QuestionnaireDocument.Find<IVariable>(entityId).DoNotExport.Should().Be(true);

        private static UpdateVariable command;
        private static Questionnaire questionnaire;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111112");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "name";
        private static string variableExpression = "expression";
        private static VariableType variableType = VariableType.Double;
    }
}
