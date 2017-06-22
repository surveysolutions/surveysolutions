using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_creating_models_with_gps_question : CodeGeneratorTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            AssemblyContext.SetupServiceLocator();

            questionnaire = Create.QuestionnaireDocument(questionnaireId, children: new[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.GpsCoordinateQuestion(Id.gA, variable: "gps", validationExpression: "gps validation", enablementCondition: "gps condition"),
                })
            });

            templateModelFactory = Create.QuestionnaireExecutorTemplateModelFactory();
        }

        private void BecauseOf() =>
            model = templateModelFactory.CreateQuestionnaireExecutorTemplateModel(questionnaire, Create.CodeGenerationSettings());

        [NUnit.Framework.Test] public void should_create_model_with_1_question () =>
            model.AllQuestions.Count.ShouldEqual(1);

        [NUnit.Framework.Test] public void should_create_questionnaire_level_with_1_question () =>
            model.QuestionnaireLevelModel.Questions.Count.ShouldEqual(1);

        [NUnit.Framework.Test] public void should_reference_same_question_model_in_AllQuestions_and_questionnaire_level () =>
            model.QuestionnaireLevelModel.Questions.First().ShouldEqual(model.AllQuestions.First());

        [NUnit.Framework.Test] public void should_create_gps_question_model () =>
        {
            QuestionTemplateModel question = model.AllQuestions.Single(x => x.Id == Id.gA);
            question.VariableName.ShouldEqual("gps");
            question.ValidationExpressions.FirstOrDefault().ValidationExpression.ShouldEqual("gps validation");
            //question.Validation.ShouldEqual("gps validation");
            question.Condition.ShouldEqual("gps condition");
            question.IsMultiOptionYesNoQuestion.ShouldBeFalse();
            question.AllMultioptionYesNoCodes.ShouldBeNull();
            question.TypeName.ShouldEqual("GeoLocation");
            question.RosterScopeName.ShouldEqual(CodeGenerator.QuestionnaireScope);
            question.ParentScopeTypeName.ShouldEqual(CodeGenerator.QuestionnaireTypeName);
        }

        private static QuestionnaireExpressionStateModelFactory templateModelFactory;
        private static QuestionnaireExpressionStateModel model;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionnaireId = Guid.Parse("ffffffffffffffffffffffffffffffff");
    }
}