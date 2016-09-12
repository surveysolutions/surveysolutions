using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [Ignore("KP-7751")]
    internal class when_answering_text_list_question_with_incorrect_roster_vector : InterviewTestsContext
    {
        private Establish context = () =>
        {
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            invalidRosterVector = new decimal[] { 3, 4 };

            questionId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = Mock.Of<IQuestionnaire>
                (_
                    => _.HasQuestion(questionId) == true
                        && _.GetQuestionType(questionId) == QuestionType.TextList
                );

            IQuestionnaireStorage questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
        };

        private Because of = () =>
            exception = Catch.Exception(() =>
                interview.AnswerTextListQuestion(
                    userId, questionId, invalidRosterVector, DateTime.Now,
                    new[]
                    {
                        new Tuple<decimal, string>(1, "Answer 1"),
                        new Tuple<decimal, string>(2, "Answer 2"),
                        new Tuple<decimal, string>(3, "Answer 3"),
                    }));

        It should_raise_InterviewException = () =>
            exception.ShouldBeOfExactType<InterviewException>();

        It should_throw_exception_with_message_containting__roster__ = () =>
            exception.Message.ToLower().ShouldContain("roster");

        It should_throw_exception_with_message_containting__information__ = () =>
            exception.Message.ToLower().ShouldContain("information");

        It should_throw_exception_with_message_containting__is_incorrect__ = () =>
            exception.Message.ToLower().ShouldContain("is incorrect");

        private static Exception exception;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId;
        private static decimal[] invalidRosterVector;
    }
}