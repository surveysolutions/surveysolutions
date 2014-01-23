using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.QuestionnaireDocumentCacheWarmerTests
{
    [Subject(typeof(QuestionnaireDocumentCacheWarmer))]
    internal class when_question_has_enablement_condition
    {
        Establish context = () =>
        {
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
                    ConditionExpression = "some expression"
                }
            });

            var expressionProcessor = new Mock<IExpressionProcessor>();
            expressionProcessor.Setup(x => x.GetIdentifiersUsedInExpression(Moq.It.IsAny<string>())).Returns(new string[] { referencedInConditionQuestionsVariableName });

            Mock.Get(ServiceLocator.Current)
              .Setup(locator => locator.GetInstance<IExpressionProcessor>())
              .Returns(expressionProcessor.Object);

            questionnaireDocumentCacheWarmer = new QuestionnaireDocumentCacheWarmer(questionnaireDocument);
        };

        Because of = () =>
            questionnaireDocumentCacheWarmer.WarmUpCaches();

        It should_QuestionsInvolvedInCustomEnablementConditionOfQuestion_contain_referenced_in_conditions_question = () =>
            questionnaireDocument.FirstOrDefault<IQuestion>(q => q.PublicKey == questionWithConditionQuestionId)
                .QuestionsInvolvedInCustomEnablementConditionOfQuestion[0].ShouldEqual(
                    new QuestionIdAndVariableName(referencedInConditionQuestionId, referencedInConditionQuestionsVariableName));

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

        private static Guid questionWithConditionQuestionId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid referencedInConditionQuestionId = new Guid("22222222222222222222222222222222");
        private static string referencedInConditionQuestionsVariableName = "var";
        private static QuestionnaireDocumentCacheWarmer questionnaireDocumentCacheWarmer;
        private static QuestionnaireDocument questionnaireDocument;
    }
}
