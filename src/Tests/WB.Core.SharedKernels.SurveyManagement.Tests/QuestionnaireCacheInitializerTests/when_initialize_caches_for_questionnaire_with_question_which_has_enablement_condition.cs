using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionnaireCacheInitializerTests
{
    internal class when_initialize_caches_for_questionnaire_with_question_which_has_enablement_condition
    {
        Establish context = () =>
        {
            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression(expression) == new[] { referencedInConditionQuestionsVariableName });

              Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IExpressionProcessor>())
                .Returns(expressionProcessor);

            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                new NumericQuestion()
                {
                    PublicKey = referencedInConditionQuestionId,
                    QuestionType = QuestionType.Numeric,
                    StataExportCaption = referencedInConditionQuestionsVariableName
                },
                new NumericQuestion()
                {
                    PublicKey = questionWithConditionQuestionId,
                    QuestionType = QuestionType.Numeric,
                    ConditionExpression = expression
                }
            });
             
            questionnaireCacheInitializer = new QuestionnaireCacheInitializer(new QuestionnaireFactory()); 
        };

        Because of = () =>
            questionnaireCacheInitializer.InitializeQuestionnaireDocumentWithCaches(questionnaireDocument);

        It should_contain_one_item_in_QuestionsInvolvedInCustomEnablementConditionOfQuestion = () =>
           questionnaireDocument.FirstOrDefault<IQuestion>(q => q.PublicKey == questionWithConditionQuestionId)
               .QuestionIdsInvolvedInCustomEnablementConditionOfQuestion.Count.ShouldEqual(1);

        It should_QuestionsInvolvedInCustomEnablementConditionOfQuestion_contain_referenced_in_conditions_question = () =>
            questionnaireDocument.FirstOrDefault<IQuestion>(q => q.PublicKey == questionWithConditionQuestionId)
                .QuestionIdsInvolvedInCustomEnablementConditionOfQuestion[0].ShouldEqual(
                    referencedInConditionQuestionId);

        private static Guid questionWithConditionQuestionId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid referencedInConditionQuestionId = new Guid("22222222222222222222222222222222");
        private static string referencedInConditionQuestionsVariableName = "var";
        private static QuestionnaireCacheInitializer questionnaireCacheInitializer;
        private static QuestionnaireDocument questionnaireDocument;
        private static string expression="some expression";

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] chapterChildren)
        {
            return new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = Guid.Parse("FFF000AAA111EE2DD2EE111AAA000FFF"),
                        Children = chapterChildren.ToList(),
                    }
                }
            };
        }
    }
}
