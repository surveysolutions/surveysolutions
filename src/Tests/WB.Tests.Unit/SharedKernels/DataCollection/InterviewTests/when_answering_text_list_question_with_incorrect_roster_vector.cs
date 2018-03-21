                        using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_text_list_question_with_incorrect_roster_vector : InterviewTestsContext
    {
         [NUnit.Framework.OneTimeSetUp] public void context () {
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
             BecauseOf();
        }

        private void BecauseOf() =>
            exception = NUnit.Framework.Assert.Throws<InterviewException>(() =>
                interview.AnswerTextListQuestion(
                    userId, questionId, invalidRosterVector, DateTime.Now,
                    new[]
                    {
                        new Tuple<decimal, string>(1, "Answer 1"),
                        new Tuple<decimal, string>(2, "Answer 2"),
                        new Tuple<decimal, string>(3, "Answer 3"),
                    }));

        [NUnit.Framework.Test] public void should_raise_InterviewException () =>
            exception.Should().BeOfType<InterviewException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__roster__ () =>
            exception.Message.ToLower().Should().Contain("roster");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__information__ () =>
            exception.Message.ToLower().Should().Contain("information");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__is_incorrect__ () =>
            exception.Message.ToLower().Should().Contain("is incorrect");

        private static Exception exception;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId;
        private static decimal[] invalidRosterVector;
    }
}
