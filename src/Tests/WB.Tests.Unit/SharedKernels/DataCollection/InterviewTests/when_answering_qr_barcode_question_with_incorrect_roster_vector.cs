using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_qr_barcode_question_with_incorrect_roster_vector : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");
            var questionnaire = Mock.Of<IQuestionnaire>
                (_
                    => _.HasQuestion(questionId) == true
                    && _.GetQuestionType(questionId) == QuestionType.QRBarcode
                );

            IQuestionnaireStorage questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            BecauseOf();
        }

        public void BecauseOf() =>
            exception = NUnit.Framework.Assert.Throws<InterviewException>(() =>
                interview.AnswerQRBarcodeQuestion(userId: userId, questionId: questionId, originDate:DateTimeOffset.Now, 
                    rosterVector: invalidRosterVector, answer: answer));

        [NUnit.Framework.Test] public void should_raise_InterviewException () =>
            exception.Should().BeOfType<InterviewException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting_words__roster_information_incorrect__ () =>
             new[] { "roster", "information", "incorrect" }.Should().OnlyContain(
                    keyword => exception.Message.ToLower().Contains(keyword));

        private static Exception exception;
        private static Interview interview;
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static decimal[] invalidRosterVector = new decimal[] { 3, 4 };
        private static string answer = "qr barcode answer";
    }
}
