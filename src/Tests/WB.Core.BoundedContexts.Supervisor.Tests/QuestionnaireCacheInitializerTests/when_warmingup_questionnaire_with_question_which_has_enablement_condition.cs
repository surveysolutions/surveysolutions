using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.QuestionnaireCacheInitializerTests
{
    internal class when_warmingup_questionnaire_with_question_which_has_enablement_condition
    {
        private Establish context = () =>
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
            expressionProcessor.Setup(x => x.GetIdentifiersUsedInExpression(Moq.It.IsAny<string>()))
                .Returns(new [] { referencedInConditionQuestionsVariableName });

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IExpressionProcessor>())
                .Returns(expressionProcessor.Object);

            questionnaireCacheInitializer = new QuestionnaireCacheInitializer(new QuestionnaireFactory());
        };

        private Because of = () =>
            questionnaireCacheInitializer.InitializeQuestionnaireDocumentWithCaches(questionnaireDocument);

        private It should_QuestionsInvolvedInCustomEnablementConditionOfQuestion_contain_referenced_in_conditions_question = () =>
            questionnaireDocument.FirstOrDefault<IQuestion>(q => q.PublicKey == questionWithConditionQuestionId)
                .QuestionIdsInvolvedInCustomEnablementConditionOfQuestion[0].ShouldEqual(
                    referencedInConditionQuestionId);

        private static Guid questionWithConditionQuestionId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid referencedInConditionQuestionId = new Guid("22222222222222222222222222222222");
        private static string referencedInConditionQuestionsVariableName = "var";
        private static QuestionnaireCacheInitializer questionnaireCacheInitializer;
        private static QuestionnaireDocument questionnaireDocument;

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
