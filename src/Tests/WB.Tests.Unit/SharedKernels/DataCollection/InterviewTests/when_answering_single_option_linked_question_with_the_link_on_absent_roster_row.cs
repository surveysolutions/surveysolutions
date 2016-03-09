using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_single_option_linked_question_with_the_link_on_absent_roster_row : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            var questionnaire = Mock.Of<IQuestionnaire>
                (_
                    => 

                        _.HasQuestion(linkedToQuestionId) == true
                       && _.GetQuestionType(linkedToQuestionId) == QuestionType.SingleOption
                       && _.GetQuestionReferencedByLinkedQuestion(linkedToQuestionId) == linkedToQuestionId
                       && _.IsQuestionLinkedToRoster(linkedToQuestionId)==true
                );
            IPlainQuestionnaireRepository questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
        };

        Because of = () =>
             exception = Catch.Exception(() => interview.AnswerSingleOptionLinkedQuestion(userId: userId, questionId: linkedToQuestionId,
                 answerTime: DateTime.Now, rosterVector: new decimal[0], selectedRosterVector: answer));

        It should_raise_InterviewException = () =>
           exception.ShouldBeOfExactType<InterviewException>();

        It should_throw_exception_with_message_containting__type_QRBarcode_expected__ = () =>
             new[] { "answer", "linked", "roster" }.ShouldEachConformTo(
                    keyword => exception.Message.ToLower().TrimEnd('.').Contains(keyword));


        private static Exception exception;
        private static Interview interview;
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid linkedToQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static decimal[] answer = {1};
    }
}