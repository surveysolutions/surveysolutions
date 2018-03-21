using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_qr_barcode_question_which_have_incorrect_type : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: questionId),
            }));

            IQuestionnaireStorage questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            BecauseOf();
        }

        public void BecauseOf() =>
             exception = NUnit.Framework.Assert.Throws<AnswerNotAcceptedException>(() =>interview.AnswerQRBarcodeQuestion(userId: userId, questionId: questionId, 
                 answerTime: DateTime.Now, rosterVector: new decimal[0], answer: answer));

        [NUnit.Framework.Test] public void should_raise_InterviewException () =>
           exception.Should().BeOfType<AnswerNotAcceptedException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__type_QRBarcode_expected__ () =>
             new [] { "type", QuestionType.QRBarcode.ToString().ToLower(), "expected" }.Should().OnlyContain(
                    keyword => exception.Message.ToLower().TrimEnd('.').Contains(keyword));


        private static Exception exception;
        private static Interview interview;
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static string answer = "some answer here";
    }
}
