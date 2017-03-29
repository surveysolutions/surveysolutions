using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    [TestFixture]
    internal class VariableTests : QuestionnaireTestsContext
    {
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void AddVariable_When_variable_title_contains_substitution_referance_Then_throws_DomainException_with_type_VariableLabelContainsSubstitutionReference()
        {
            // arrange
            Guid responsibleId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            AddVariable addVariableCommand = Create.Command.AddVariable(
                questionnaireId: questionnaire.Id, 
                entityId: Guid.NewGuid(), 
                parentId: questionnaire.QuestionnaireDocument.Children.First().PublicKey,
                label: " %substitution% test",
                responsibleId: responsibleId);

            // act
            TestDelegate act = () => questionnaire.AddVariableAndMoveIfNeeded(addVariableCommand);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.VariableLabelContainsSubstitutionReference));
        }

        [Test]
        public void UpdateeVariable_When_variable_title_contains_substitution_referance_Then_throws_DomainException_with_type_VariableLabelContainsSubstitutionReference()
        {
            // arrange
            Guid responsibleId = Guid.NewGuid();
            Guid variableId = Guid.NewGuid();
            Questionnaire questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddVariable(
                entityId: variableId,
                parentId: questionnaire.QuestionnaireDocument.Children.First().PublicKey,
                responsibleId: responsibleId);

            UpdateVariable updateVariableCommand = Create.Command.UpdateVariable(
                questionnaireId: questionnaire.Id,
                entityId: variableId,
                type: VariableType.Boolean, 
                name: "name",
                expression: "true",
                label: " %substitution% test",
                userId: responsibleId);

            // act
            TestDelegate act = () => questionnaire.UpdateVariable(updateVariableCommand);

            // assert
            var domainException = Assert.Throws<QuestionnaireException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.VariableLabelContainsSubstitutionReference));
        }
    }
}