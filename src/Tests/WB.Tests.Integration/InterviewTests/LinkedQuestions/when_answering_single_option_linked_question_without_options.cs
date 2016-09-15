using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_single_option_linked_question_without_options : InterviewTestsContext
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
        };

        Because of = () =>
             exception = Catch.Exception(() => interview.AnswerSingleOptionLinkedQuestion(userId: userId, questionId: linkedToQuestionId,
                 answerTime: DateTime.Now, rosterVector: new decimal[0], selectedRosterVector: answer));

        It should_raise_InterviewException = () =>
           exception.ShouldBeOfExactType<InterviewException>();

        It should_throw_exception_with_message_containting__type_QRBarcode_expected__ = () =>
             new[] { "answer", "linked", "options" }.ShouldEachConformTo(
                    keyword => exception.Message.ToLower().TrimEnd('.').Contains(keyword));


        private static Exception exception;
        private static Interview interview;
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid linkedToQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static decimal[] answer = {1};
    }
}