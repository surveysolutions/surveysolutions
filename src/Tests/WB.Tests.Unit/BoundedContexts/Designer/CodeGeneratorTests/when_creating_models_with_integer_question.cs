using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Tests.Unit.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_creating_models_with_integer_question : CodeGeneratorTestsContext
    {
        Establish context = () =>
        {
            AssemblyContext.SetupServiceLocator();

            questionnaire = Create.QuestionnaireDocument(questionnaireId, children: new[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(Id.gA, variable: "integer", validationExpression: "integer validation", enablementCondition: "integer condition"),
                })
            });

            templateModelFactory = Create.QuestionnaireExecutorTemplateModelFactory();
        };

        Because of = () =>
            model = templateModelFactory.CreateQuestionnaireExecutorTemplateModel(questionnaire, Create.CodeGenerationSettings());

        It should_create_model_with_1_question = () =>
            model.AllQuestions.Count.ShouldEqual(1);

        It should_create_questionnaire_level_with_1_question = () =>
            model.QuestionnaireLevelModel.Questions.Count.ShouldEqual(1);

        It should_reference_same_question_model_in_AllQuestions_and_questionnaire_level = () =>
            model.QuestionnaireLevelModel.Questions.First().ShouldEqual(model.AllQuestions.First());

        It should_create_integer_question_model = () =>
        {
            QuestionTemplateModel question = model.AllQuestions.Single(x => x.Id == Id.gA);
            question.VariableName.ShouldEqual("integer");
            question.AllMultioptionYesNoCodes.ShouldBeNull();
            question.Condition.ShouldEqual("integer condition");
            question.Validation.ShouldEqual("integer validation");
            question.IsMultiOptionYesNoQuestion.ShouldEqual(false);
            question.TypeName.ShouldEqual("long?");
            question.RosterScopeName.ShouldEqual(CodeGenerator.QuestionnaireScope);
            question.ParentScopeTypeName.ShouldEqual(CodeGenerator.QuestionnaireTypeName);
        };

        private static QuestionnaireExpressionStateModelFactory templateModelFactory;
        private static QuestionnaireExpressionStateModel model;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionnaireId = Guid.Parse("ffffffffffffffffffffffffffffffff");
    }
}