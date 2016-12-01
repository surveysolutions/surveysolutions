using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_qr_barcode_question_which_is_absent : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");
            var questionnaire = Mock.Of<IQuestionnaire>
            (_
                => _.HasQuestion(questionId) == false
            );

            IQuestionnaireStorage questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
        };

        Because of = () =>
             exception = Catch.Exception(() =>interview.AnswerQRBarcodeQuestion(userId: userId, questionId: questionId, 
                 answerTime: DateTime.Now, rosterVector: new decimal[0], answer: answer));

        It should_raise_InterviewException = () =>
           exception.ShouldBeOfExactType<InterviewException>();

        It should_throw_exception_with_message_containting__question____is____missing__ = () =>
             new[] { "question", "is", "missing" }.ShouldEachConformTo(
                    keyword => exception.Message.ToLower().Contains(keyword));


        private static Exception exception;
        private static Interview interview;
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static string answer = "some answer here";
    }
}