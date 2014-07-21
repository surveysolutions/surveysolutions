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

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    [Ignore("C#")]
    internal class when_answering_qr_barcode_question_and_question_is_disabled : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");
            var questionnaire = Mock.Of<IQuestionnaire>
                (_
                    => _.HasQuestion(questionId) == true &&
                        _.GetQuestionType(questionId) == QuestionType.QRBarcode &&
                        _.GetAllQuestionsWithNotEmptyCustomEnablementConditions() == new Guid[] { questionId } 
                );

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire));

            interview = CreateInterview(questionnaireId: questionnaireId);
        };

        Because of = () =>
             exception = Catch.Exception(() =>interview.AnswerQRBarcodeQuestion(userId: userId, questionId: questionId, 
                 answerTime: DateTime.Now, rosterVector: new decimal[0], answer: answer));

        It should_raise_InterviewException = () =>
           exception.ShouldBeOfType<InterviewException>();

        It should_throw_exception_with_message_containting__question_disabled__ = () =>
             new [] { "question", "disabled" }.ShouldEachConformTo(
                    keyword => exception.Message.ToLower().TrimEnd('.').Contains(keyword));


        private static Exception exception;
        private static Interview interview;
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static string answer = "some answer here";
    }
}