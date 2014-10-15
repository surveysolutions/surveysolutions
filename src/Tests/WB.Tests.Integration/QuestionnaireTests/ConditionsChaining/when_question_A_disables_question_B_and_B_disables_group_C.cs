using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.ExpressionProcessor.Implementation.Services;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using WB.Tests.Integration.InterviewTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.QuestionnaireTests.ConditionsChaining
{
    [Ignore("C#")]
    internal class when_question_A_disables_question_B_and_B_disables_group_C : InterviewTestsContext
    {
        Establish context = () =>
        {
            SetupInstanceToMockedServiceLocator<IExpressionProcessor>(expressionProcessorMock.Object);

            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                questionX = new NumericQuestion("QuestionX")
                {
                    PublicKey = Guid.NewGuid(),
                    StataExportCaption = "QuestionX",
                    QuestionType = QuestionType.Numeric,
                    IsInteger = true,
                },
                new Group("GroupA")
                {
                    ConditionExpression = "[QuestionX] > 0",
                    Children = new List<IComposite>
                    {
                        (question1 = new NumericQuestion("Question1")
                        {
                            PublicKey = Guid.Parse("21111111111111111111111111111111"),
                            QuestionType = QuestionType.Numeric,
                            IsInteger = true,
                            StataExportCaption = "Question1",
                        })
                    }
                },
                new Group("GroupB")
                {
                    ConditionExpression = "[QuestionX] > 0 and [Question1] = 1",
                    Children = new List<IComposite>
                    {
                        new NumericQuestion("Question2")
                        {
                            PublicKey = Guid.Parse("22222222222222222222222222222222"),
                            QuestionType = QuestionType.Numeric,
                            ConditionExpression = question2EnablementCondition,
                            IsInteger = true,
                            StataExportCaption = "Question2",
                        }
                    }
                });

            interview = CreateInterviewFromQuestionnaireDocumentRegisteringAllNeededDependencies(questionnaireDocument);
            interview.Apply(new NumericRealQuestionAnswered(Guid.NewGuid(), question1.PublicKey, new decimal[]{}, DateTime.Now, 1));
        };

        Because of = () => interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), questionX.PublicKey, new decimal[] { }, DateTime.Now, 1);

        It should_calculate_conditions_on_questions_only_once = () => expressionProcessorMock.Verify(x => x.EvaluateBooleanExpression(question2EnablementCondition, Moq.It.IsAny<Func<string, object>>()), Times.Once);

        static Interview interview;
        static NumericQuestion questionX;
        static NumericQuestion question1;
        static Mock<ExpressionProcessor> expressionProcessorMock = new Mock<ExpressionProcessor>(){CallBase = true};
        static QuestionnaireDocument questionnaireDocument;
        private static string question2EnablementCondition = "[Question1] > 20 and [QuestionX] > 0";
    }
}