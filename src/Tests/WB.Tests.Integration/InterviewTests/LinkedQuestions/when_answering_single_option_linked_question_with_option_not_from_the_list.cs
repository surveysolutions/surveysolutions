using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_single_option_linked_question_with_option_not_from_the_list : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            var triggerQuestionId = Guid.NewGuid();
            var titleQuestionId = Guid.NewGuid();
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new IComposite[]
            {
                Create.SingleQuestion(id: linkedToQuestionId, linkedToQuestionId: titleQuestionId,
                    variable: "link_single"),
                Integration.Create.NumericIntegerQuestion(id: triggerQuestionId, variable: "num_trigger"),
                Create.Roster(id: Guid.NewGuid(), rosterSizeSourceType: RosterSizeSourceType.Question,
                    rosterSizeQuestionId: triggerQuestionId, rosterTitleQuestionId: titleQuestionId, variable: "ros1",
                    children: new IComposite[]
                    {
                        Create.NumericRealQuestion(id: titleQuestionId, variable: "link_source")
                    })
            });

            interview = SetupInterview(questionnaireDocument: questionnaireDocument);

            interview.AnswerNumericIntegerQuestion(userId: userId, questionId: triggerQuestionId,
                 answerTime: DateTime.Now, rosterVector: new decimal[0], answer: 1);
            interview.AnswerNumericRealQuestion(userId: userId, questionId: titleQuestionId,
                answerTime: DateTime.Now, rosterVector: new decimal[] {0}, answer: 2.3);
        };

        Because of = () =>
             exception = Catch.Exception(() => interview.AnswerSingleOptionLinkedQuestion(userId: userId, questionId: linkedToQuestionId,
                 answerTime: DateTime.Now, rosterVector: new decimal[0], selectedRosterVector: answer));

        It should_raise_InterviewException = () =>
           exception.ShouldBeOfExactType<InterviewException>();

        It should_throw_exception_with_message_containting__type_QRBarcode_expected__ = () =>
             new[] { "answer", "linked", "options", "absent" }.ShouldEachConformTo(
                    keyword => exception.Message.ToLower().TrimEnd('.').Contains(keyword));


        private static Exception exception;
        private static Interview interview;
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid linkedToQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static decimal[] answer = { 1 };
    }
}