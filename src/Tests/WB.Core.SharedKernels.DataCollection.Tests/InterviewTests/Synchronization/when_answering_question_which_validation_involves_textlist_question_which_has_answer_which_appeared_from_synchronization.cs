using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests.Synchronization
{
    [Ignore("C#")]
    internal class when_answering_question_which_validation_involves_textlist_question_which_has_answer_which_appeared_from_synchronization : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDD0000000000000000");
            userId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var textListQuestion = Guid.Parse("11111111111111111111111111111111");
            answeredQuestion = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Mock.Of<IQuestionnaire>
            (_
                => _.HasQuestion(textListQuestion) == true
                && _.GetQuestionType(textListQuestion) == QuestionType.TextList

                && _.HasQuestion(answeredQuestion) == true
                && _.GetQuestionType(answeredQuestion) == QuestionType.Text
                && _.IsCustomValidationDefined(answeredQuestion) == true
               // && _.GetQuestionsInvolvedInCustomValidation(answeredQuestion) == new[] { textListQuestion, answeredQuestion }

                && _.GetQuestionVariableName(answeredQuestion) == answeredQuestiontVariableName
                && _.GetQuestionVariableName(textListQuestion) == textlistVariableName
            );

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire));

            var expressionProcessor = Mock.Of<SharedKernels.ExpressionProcessor.Services.IExpressionProcessor>();

            Mock.Get(expressionProcessor)
                .Setup(_ => _.EvaluateBooleanExpression(it.IsAny<string>(), it.IsAny<Func<string, object>>()))
                .Callback<string, Func<string, object>>((expression, getValueForIdentifier) =>
                {
                    textListAnswerProvidedToExpressionProcessor = getValueForIdentifier(textlistVariableName);
                });

            SetupInstanceToMockedServiceLocator<SharedKernels.ExpressionProcessor.Services.IExpressionProcessor>(expressionProcessor);

            Tuple<decimal, string>[] textListAnswer = new[] { Tuple.Create((decimal) 1, "one"), Tuple.Create((decimal) 2, "two") };

            InterviewSynchronizationDto interviewSynchronizationData = CreateInterviewSynchronizationDto(
                questionnaireId: questionnaireId,
                questionnaireVersion: questionnaire.Version,
                answers: new[] { new AnsweredQuestionSynchronizationDto(textListQuestion, EmptyRosterVector, textListAnswer, null) });

            interview = CreateInterview(questionnaireId: questionnaireId);
            interview.Apply(new InterviewSynchronized(interviewSynchronizationData));
        };

        Because of = () =>
            interview.AnswerTextQuestion(userId, answeredQuestion, EmptyRosterVector, DateTime.Now, "answer");

        It should_specify_null_as_value_of_textlist_answer_to_expression_processor = () =>
            textListAnswerProvidedToExpressionProcessor.ShouldBeNull();

        private static object textListAnswerProvidedToExpressionProcessor = "default not null value";
        private static Interview interview;

        private static Guid userId;
        private static Guid answeredQuestion;
        private static readonly decimal[] EmptyRosterVector = new decimal[]{};
        private static string textlistVariableName = "textlist";
        private static string answeredQuestiontVariableName = "text";
    }
}