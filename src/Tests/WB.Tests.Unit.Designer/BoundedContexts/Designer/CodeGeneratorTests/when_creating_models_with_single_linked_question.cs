using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_creating_models_with_single_linked_question : CodeGeneratorTestsContext
    {
        Establish context = () =>
        {
            AssemblyContext.SetupServiceLocator();

            questionnaire = Create.QuestionnaireDocument(questionnaireId, children: new[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.SingleQuestion(Id.gA, variable: "singleLinked", linkedToQuestionId: Id.g10, validationExpression: "singleLinked validation", enablementCondition: "singleLinked condition"),
                    Create.Roster(rosterId: Id.gB, variable: "fixed_roster", rosterType: RosterSizeSourceType.FixedTitles, fixedTitles: new string[] {"1", "2"}, enablementCondition: "roster condition", children: new IComposite[]
                    {
                        Create.TextQuestion(questionId: Id.gC, variable: "linkedSource")
                    })
                })
            });

            templateModelFactory = Create.QuestionnaireExecutorTemplateModelFactory();
        };

        Because of = () =>
            model = templateModelFactory.CreateQuestionnaireExecutorTemplateModel(questionnaire, Create.CodeGenerationSettings());

        It should_create_model_with_2_questions = () =>
            model.AllQuestions.Count.ShouldEqual(2);

        It should_create_questionnaire_level_with_1_question = () =>
            model.QuestionnaireLevelModel.Questions.Count.ShouldEqual(1);

        It should_reference_same_question_model_in_AllQuestions_and_questionnaire_level = () =>
            model.QuestionnaireLevelModel.Questions.First().ShouldEqual(model.AllQuestions.First());

        It should_create_singleLinked_question_model = () =>
        {
            QuestionTemplateModel question = model.AllQuestions.Single(x => x.Id == Id.gA);
            question.VariableName.ShouldEqual("singleLinked");
            question.ValidationExpressions.FirstOrDefault().ValidationExpression.ShouldEqual("singleLinked validation");
            question.Condition.ShouldEqual("singleLinked condition");
            question.IsMultiOptionYesNoQuestion.ShouldBeFalse();
            question.AllMultioptionYesNoCodes.ShouldBeNull();
            question.TypeName.ShouldEqual("decimal[]");
            question.RosterScopeName.ShouldEqual(CodeGenerator.QuestionnaireScope);
            question.ParentScopeTypeName.ShouldEqual(CodeGenerator.QuestionnaireTypeName);
        };

        private static QuestionnaireExpressionStateModelFactory templateModelFactory;
        private static QuestionnaireExpressionStateModel model;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionnaireId = Guid.Parse("ffffffffffffffffffffffffffffffff");
    }
}