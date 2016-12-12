using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_creating_executor_template_model_for_questionnaire_with_linked_to_list_questions : CodeGeneratorTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                Create.TextListQuestion(listQuestionId)   ,
                Create.SingleQuestion(singleLinkedToListId, linkedToQuestionId: listQuestionId),
                Create.MultyOptionsQuestion(multiLiskedToListId, linkedToQuestionId: listQuestionId)
            });

            expressionStateModelFactory = Create.QuestionnaireExecutorTemplateModelFactory();
        };

        Because of = () =>
            templateModel = expressionStateModelFactory.CreateQuestionnaireExecutorTemplateModel(questionnaire, Create.CodeGenerationSettings());

        It should_generate_model_for_single_linked_to_list_question_with_type__nullable_decimal = () =>
        {
            var singleLinkedToList = GetQuestion(singleLinkedToListId);
            singleLinkedToList.TypeName.ShouldEqual("decimal?");
        };

        It should_generate_model_for_multi_linked_to_list_question_with_type__decimal = () =>
        {
            var multiLiskedToList = GetQuestion(multiLiskedToListId);
            multiLiskedToList.TypeName.ShouldEqual("decimal[]");
        };

        private static QuestionTemplateModel GetQuestion(Guid id)
        {
            return templateModel.AllQuestions.Single(x => x.Id == id);
        }

        private static QuestionnaireExpressionStateModelFactory expressionStateModelFactory;
        private static QuestionnaireExpressionStateModel templateModel;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid listQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid singleLinkedToListId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid multiLiskedToListId = Guid.Parse("33333333333333333333333333333333");
    }
}