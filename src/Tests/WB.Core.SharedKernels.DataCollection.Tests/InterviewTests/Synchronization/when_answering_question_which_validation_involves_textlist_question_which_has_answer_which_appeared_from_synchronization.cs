using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests.Synchronization
{
    internal class when_answering_question_which_validation_involves_textlist_question_which_has_answer_which_appeared_from_synchronization : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDD0000000000000000");
            userId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var textListQuestion = new QuestionIdAndVariableName(Guid.Parse("11111111111111111111111111111111"), "textlist");
            answeredQuestion = new QuestionIdAndVariableName(Guid.Parse("22222222222222222222222222222222"), "text");

            var questionnaire = Mock.Of<IQuestionnaire>
            (_
                => _.HasQuestion(textListQuestion.Id) == true
                && _.GetQuestionType(textListQuestion.Id) == QuestionType.TextList

                && _.HasQuestion(answeredQuestion.Id) == true
                && _.GetQuestionType(answeredQuestion.Id) == QuestionType.Text
                && _.IsCustomValidationDefined(answeredQuestion.Id) == true
                && _.GetQuestionsInvolvedInCustomValidation(answeredQuestion.Id) == new[] { textListQuestion, answeredQuestion }
            );

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire));

            var expressionProcessor = Mock.Of<IExpressionProcessor>();

            Mock.Get(expressionProcessor)
                .Setup(_ => _.EvaluateBooleanExpression(it.IsAny<string>(), it.IsAny<Func<string, object>>()))
                .Callback<string, Func<string, object>>((expression, getValueForIdentifier) =>
                {
                    textListAnswerProvidedToExpressionProcessor = getValueForIdentifier(textListQuestion.VariableName);
                });

            SetupInstanceToMockedServiceLocator<IExpressionProcessor>(expressionProcessor);

            Tuple<decimal, string>[] textListAnswer = new[] { Tuple.Create((decimal) 1, "one"), Tuple.Create((decimal) 2, "two") };

            InterviewSynchronizationDto interviewSynchronizationData = CreateInterviewSynchronizationDto(
                questionnaireId: questionnaireId,
                questionnaireVersion: questionnaire.Version,
                answers: new[] { new AnsweredQuestionSynchronizationDto(textListQuestion.Id, EmptyRosterVector, textListAnswer, null) });

            interview = CreateInterview(questionnaireId: questionnaireId);
            interview.Apply(new InterviewSynchronized(interviewSynchronizationData));
        };

        Because of = () =>
            interview.AnswerTextQuestion(userId, answeredQuestion.Id, EmptyRosterVector, DateTime.Now, "answer");

        It should_specify_null_as_value_of_textlist_answer_to_expression_processor = () =>
            textListAnswerProvidedToExpressionProcessor.ShouldBeNull();

        private static object textListAnswerProvidedToExpressionProcessor = "default not null value";
        private static Interview interview;

        private static Guid userId;
        private static QuestionIdAndVariableName answeredQuestion;
        private static readonly decimal[] EmptyRosterVector = new decimal[]{};
    }
}