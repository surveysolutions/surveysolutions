using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [NUnit.Framework.Ignore("C#, KP-4388 Different question types without validation expressions (barcode)")]
    internal class when_answering_qr_barcode_question_and_answer_is_specified : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");
            var questionnaire = Mock.Of<IQuestionnaire>
                (_
                    => _.HasQuestion(questionId) == true &&
                        _.GetQuestionType(questionId) == QuestionType.QRBarcode
                );

            IQuestionnaireStorage questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            eventContext = new EventContext();
        }

        public void BecauseOf() =>
            interview.AnswerQRBarcodeQuestion(userId: userId, questionId: questionId, originDate: answerTime,
                                              rosterVector: propagationVector, answer: answer);

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        [NUnit.Framework.Test] public void should_raise_QRBarcodeQuestionAnswered_event () =>
            eventContext.ShouldContainEvent<QRBarcodeQuestionAnswered>();

        [NUnit.Framework.Test] public void should_raise_ValidityChanges_event () =>
            eventContext.ShouldContainEvent<AnswersDeclaredValid>();

        [NUnit.Framework.Test] public void should_raise_QRBarcodeQuestionAnswered_event_with_QuestionId_equal_to_questionId () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionAnswered>().QuestionId.Should().Be(questionId);

        [NUnit.Framework.Test] public void should_raise_QRBarcodeQuestionAnswered_event_with_UserId_equal_to_userId () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionAnswered>().UserId.Should().Be(userId);

        [NUnit.Framework.Test] public void should_raise_QRBarcodeQuestionAnswered_event_with_PropagationVector_equal_to_propagationVector () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionAnswered>().RosterVector.Should().BeEquivalentTo(propagationVector);

        [NUnit.Framework.Test] public void should_raise_QRBarcodeQuestionAnswered_event_with_AnswerTime_equal_to_answerTime () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionAnswered>().AnswerTimeUtc.Should().Be(answerTime.UtcDateTime);

        [NUnit.Framework.Test] public void should_raise_QRBarcodeQuestionAnswered_event_with_Answer_equal_to_answer () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionAnswered>().Answer.Should().Be(answer);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static decimal[] propagationVector = new decimal[0];
        private static DateTimeOffset answerTime = DateTimeOffset.Now;
        private static string answer = "some answer here";
    }
}
