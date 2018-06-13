using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_creating_models_with_date_question : CodeGeneratorTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            AssemblyContext.SetupServiceLocator();

            questionnaire = Create.QuestionnaireDocument(questionnaireId, children: new[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.DateTimeQuestion(Id.gA, variable: "date", validationExpression: "date validation", enablementCondition: "date condition"),
                })
            });

            templateModelFactory = Create.QuestionnaireExecutorTemplateModelFactory();
            BecauseOf();
        }

        private void BecauseOf() =>
            model = templateModelFactory.CreateQuestionnaireExecutorTemplateModel(questionnaire, Create.CodeGenerationSettings());

        [NUnit.Framework.Test] public void should_create_model_with_1_question () =>
            model.AllQuestions.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_create_questionnaire_level_with_1_question () =>
            model.QuestionnaireLevelModel.Questions.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_reference_same_question_model_in_AllQuestions_and_questionnaire_level () =>
            model.QuestionnaireLevelModel.Questions.First().Should().Be(model.AllQuestions.First());

        [NUnit.Framework.Test] public void should_create_date_question_model () 
        {
            QuestionTemplateModel question = model.AllQuestions.Single(x => x.Id == Id.gA);
            question.VariableName.Should().Be("date");
            question.ValidationExpressions.FirstOrDefault().ValidationExpression.Should().Be("date validation");
            question.Condition.Should().Be("date condition");
            question.IsMultiOptionYesNoQuestion.Should().BeFalse();
            question.AllMultioptionYesNoCodes.Should().BeNull();
            question.TypeName.Should().Be("DateTime?");
            question.RosterScopeName.Should().Be(CodeGenerator.QuestionnaireScope);
            question.ParentScopeTypeName.Should().Be(CodeGenerator.QuestionnaireTypeName);
        }

        private static QuestionnaireExpressionStateModelFactory templateModelFactory;
        private static QuestionnaireExpressionStateModel model;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionnaireId = Guid.Parse("ffffffffffffffffffffffffffffffff");
    }
}