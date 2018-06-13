using System;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_real_question_with_allowed_3_decimal_places_but_answer_has_4_decimal_places : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            realQuestionId = Guid.Parse("11111111111111111111111111111111");


            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
            {
                Create.Entity.NumericRealQuestion(id: realQuestionId, countOfDecimalPlaces: 3),
            }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            BecauseOf();
        }

        public void BecauseOf() => expectedException = NUnit.Framework.Assert.Throws<AnswerNotAcceptedException>(() =>
            interview.AnswerNumericRealQuestion(userId, realQuestionId, RosterVector.Empty, DateTime.Now, 0.1234));

        [NUnit.Framework.Test] public void should_throw_AnswerNotAcceptedException () =>
            expectedException.Should().BeOfType(typeof (AnswerNotAcceptedException));

        [NUnit.Framework.Test] public void should_throw_exception_with_specific_words () =>
            expectedException.Message.ToSeparateWords().Should().Contain("allowed", "decimal", "places");

        private static Exception expectedException;
        private static Guid realQuestionId;
        private static Interview interview;
        private static Guid userId;
    }
}
