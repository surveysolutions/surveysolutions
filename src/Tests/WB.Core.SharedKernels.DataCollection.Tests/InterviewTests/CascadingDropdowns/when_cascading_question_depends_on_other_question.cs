using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests.CascadingDropdowns
{
    internal class when_cascading_question_depends_on_other_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            parentSingleOptionQuestionId = Guid.Parse("9E96D4AB-DF91-4FC9-9585-23FA270B25D7");
            childCascadedComboboxId = Guid.Parse("C6CC807A-3E81-406C-A110-1044AE3FD89B");
            actorId = Guid.Parse("366404A9-374E-4DCC-9B56-1F15103DE880");
            questionnaireId = Guid.Parse("3B7145CD-A235-44D0-917C-7B34A1017AEC");
            numericQuestionId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            grandChildCascading = Guid.Parse("107E3563-8005-41D3-8073-06DCDA4C528D");

            var conditionExpression = "[numeric]==2";

            var numericQuestionVariable = "numeric";

            var expressionProcessor = new Mock<IExpressionProcessor>();
            expressionProcessor.Setup(x => x.GetIdentifiersUsedInExpression(conditionExpression))
                .Returns(new[] { numericQuestionVariable });
            expressionProcessor.Setup(x => x.EvaluateBooleanExpression(conditionExpression, Moq.It.IsAny<Func<string, object>>()))
                .Returns(true);

            Mock.Get(ServiceLocator.Current)
               .Setup(locator => locator.GetInstance<IExpressionProcessor>())
               .Returns(expressionProcessor.Object);

            questionnaire = Create.Questionnaire(actorId,
                CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion
                {
                    PublicKey = numericQuestionId,
                    QuestionType = QuestionType.Numeric,
                    StataExportCaption = numericQuestionVariable,
                    IsInteger = true
                },
                new SingleQuestion
                {
                    PublicKey = parentSingleOptionQuestionId,
                    QuestionType = QuestionType.SingleOption,
                    StataExportCaption = "parent",
                    Answers = new List<Answer>
                    {
                        new Answer { AnswerText = "one", AnswerValue = "1", PublicKey = Guid.NewGuid() },
                        new Answer { AnswerText = "two", AnswerValue = "2", PublicKey = Guid.NewGuid() }
                    }
                },
                    new SingleQuestion
                    {
                        PublicKey = childCascadedComboboxId,
                        QuestionType = QuestionType.SingleOption,
                        CascadeFromQuestionId = parentSingleOptionQuestionId,
                        StataExportCaption = "child",
                        Mandatory = true,
                        Answers = new List<Answer>
                        {
                            new Answer { AnswerText = "child 1", AnswerValue = "1", PublicKey = Guid.NewGuid(), ParentValue = "1" },
                            new Answer { AnswerText = "child 2", AnswerValue = "2", PublicKey = Guid.NewGuid(), ParentValue = "2" }
                        }
                    },
                     new SingleQuestion
                     {
                         PublicKey = grandChildCascading,
                         QuestionType = QuestionType.SingleOption,
                         CascadeFromQuestionId = childCascadedComboboxId,
                         StataExportCaption = "grandChild",
                         ConditionExpression = conditionExpression,
                         Answers = new List<Answer>
                        {
                            new Answer { AnswerText = "child 1", AnswerValue = "1", PublicKey = Guid.NewGuid(), ParentValue = "1" },
                            new Answer { AnswerText = "child 2", AnswerValue = "2", PublicKey = Guid.NewGuid(), ParentValue = "2" }
                        }
                     }
                    ));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire.GetQuestionnaire());

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            interview = CreateInterview(questionnaireId: questionnaireId);

            eventContext = new EventContext();
        };

        Because of = () => interview.AnswerNumericIntegerQuestion(actorId, numericQuestionId, new decimal[]{}, DateTime.Now, 2);

        It should_not_enable_grand_child_question = () => eventContext.ShouldNotContainEvent<QuestionsEnabled>(e => e.Questions.Any(q => q.Id == grandChildCascading));

        static Guid parentSingleOptionQuestionId;
        static Guid childCascadedComboboxId;
        static Guid actorId;
        static Guid questionnaireId;
        static Questionnaire questionnaire;
        static EventContext eventContext;
        static Interview interview;
        static Guid numericQuestionId;
        private static Guid grandChildCascading;
    }
}

