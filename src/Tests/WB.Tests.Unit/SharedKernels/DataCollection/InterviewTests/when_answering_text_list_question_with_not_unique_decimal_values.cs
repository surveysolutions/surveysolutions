using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_text_list_question_with_not_unique_decimal_values : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextListQuestion(questionId: questionId),
            }));

            IQuestionnaireStorage questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            BecauseOf();
        }

        private void BecauseOf() =>
            exception = NUnit.Framework.Assert.Throws<InterviewException>(() =>
                interview.AnswerTextListQuestion(
                    userId, questionId, rosterVector, DateTime.Now,
                    new[]
                    {
                        new Tuple<decimal, string>(1.5m, "Answer 1"),
                        new Tuple<decimal, string>(1.5m, "Answer 2"),
                        new Tuple<decimal, string>(1.2m, "Answer 3"),
                    }));

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__decimal_values__ () =>
            exception.Message.ToLower().Should().Contain("decimal values");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__should_be_unique__ () =>
            exception.Message.ToLower().Should().Contain("should be unique");

        private static Exception exception;
        private static Interview interview;
        private static Guid userId= Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static decimal[] rosterVector = new decimal[] { };
    }
}
