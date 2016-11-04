using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_creating_executor_template_model_for_questionnaire_with_multimedia_question : CodeGeneratorTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(
                new IComposite[]
                {
                    new Group
                    {
                        Title = "Chapter",
                        PublicKey = chapterId,
                        Children = new List<IComposite>
                        {
                            new TextQuestion
                            {
                                PublicKey = textId,
                                QuestionType = QuestionType.Text,
                                StataExportCaption = "text"
                            },
                            new MultimediaQuestion
                            {
                                PublicKey = multimediaId,
                                QuestionType = QuestionType.Multimedia,
                                StataExportCaption = "multimedia",
                                ConditionExpression = "text == \"a\""
                            }
                        }.ToReadOnlyCollection()
                    },
                });

            expressionStateModelFactory = Create.QuestionnaireExecutorTemplateModelFactory();
        };

        Because of = () =>
            templateModel = expressionStateModelFactory.CreateQuestionnaireExecutorTemplateModel(questionnaire, Create.CodeGenerationSettings());

        It should_generate_model_for_multimedia_question = () =>
            GetQuestion(multimediaId).ShouldNotBeNull();

        It should_generate_model_for_multimedia_question_with_specified_values = () =>
            GetQuestion(multimediaId).ShouldContainValues(
                id: multimediaId,
                variableName: "multimedia",
                conditions: "text == \"a\"",
                validations: null,
                questionType: QuestionType.Multimedia,
                generatedIdName: "@__multimedia__id",
                generatedTypeName: "string",
                generatedMemberName: "@__multimedia",
                generatedStateName: "@__multimedia__state",
                rosterScopeName: "@__questionnaire_scope",
                generatedValidationsMethodName: "IsValid__multimedia",
                generatedConditionsMethodName: "IsEnabled__multimedia");


        It should_generate_model_for_text_question = () =>
            GetQuestion(textId).ShouldNotBeNull();

        private static QuestionTemplateModel GetQuestion(Guid id)
        {
            return templateModel.AllQuestions.Single(x => x.Id == id);
        }

        private static QuestionnaireExpressionStateModelFactory expressionStateModelFactory;
        private static QuestionnaireExpressionStateModel templateModel;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid chapterId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid multimediaId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid textId = Guid.Parse("33333333333333333333333333333333");
    }
}