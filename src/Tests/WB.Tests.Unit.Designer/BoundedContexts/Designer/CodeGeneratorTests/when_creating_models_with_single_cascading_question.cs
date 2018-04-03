using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_creating_models_with_single_cascading_question : CodeGeneratorTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            AssemblyContext.SetupServiceLocator();

            questionnaire = Create.QuestionnaireDocument(questionnaireId, children: new[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.SingleQuestion(Id.gA, variable: "single", validationExpression: "single validation", enablementCondition: "single condition", options: new List<Answer>
                    {
                        Create.Option("1", "Option 1"),
                        Create.Option("2", "Option 2")
                    }),
                    Create.SingleQuestion(Id.gB, variable: "singleCascading", cascadeFromQuestionId:Id.gA, validationExpression: "singleCascading validation", enablementCondition: "singleCascading condition",options: new List<Answer>
                    {
                        Create.Option("11", "Option 1.1", "1"),
                        Create.Option("12", "Option 1.2", "1"),
                        Create.Option("21", "Option 2.1", "2"),
                        Create.Option("22", "Option 2.2", "2"),
                    })
                })
            });

            templateModelFactory = Create.QuestionnaireExecutorTemplateModelFactory();
            BecauseOf();
        }

        private void BecauseOf() =>
            model = templateModelFactory.CreateQuestionnaireExecutorTemplateModel(questionnaire, Create.CodeGenerationSettings());

        [NUnit.Framework.Test] public void should_create_model_with_2_questions () =>
            model.AllQuestions.Count.Should().Be(2);

        [NUnit.Framework.Test] public void should_create_questionnaire_level_with_2_questions () =>
            model.QuestionnaireLevelModel.Questions.Count.Should().Be(2);

        [NUnit.Framework.Test] public void should_reference_same_question_model_in_AllQuestions_and_questionnaire_level () =>
            model.QuestionnaireLevelModel.Questions.Last().Should().Be(model.AllQuestions.Last());

        [NUnit.Framework.Test] public void should_create_single_cascading_question_model () 
        {
            QuestionTemplateModel question = model.AllQuestions.Single(x => x.Id == Id.gB);
            question.VariableName.Should().Be("singleCascading");
            question.ValidationExpressions.FirstOrDefault().ValidationExpression.Should().Be("singleCascading validation");
            question.Condition.Should().Be("!IsAnswerEmpty(single) && singleCascading condition");
            question.IsMultiOptionYesNoQuestion.Should().Be(false);
            question.AllMultioptionYesNoCodes.Should().BeNull();
            question.TypeName.Should().Be("decimal?");
            question.RosterScopeName.Should().Be(CodeGenerator.QuestionnaireScope);
            question.ParentScopeTypeName.Should().Be(CodeGenerator.QuestionnaireTypeName);
        }

        private static QuestionnaireExpressionStateModelFactory templateModelFactory;
        private static QuestionnaireExpressionStateModel model;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionnaireId = Guid.Parse("ffffffffffffffffffffffffffffffff");
    }
}