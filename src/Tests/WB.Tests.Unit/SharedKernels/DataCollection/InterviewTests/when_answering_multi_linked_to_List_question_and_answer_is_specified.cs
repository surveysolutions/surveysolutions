using System;
using FluentAssertions;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_multi_linked_to_List_question_and_answer_is_specified : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextListQuestion(questionId: questionId),
                Create.Entity.MultipleOptionsQuestion(linkedQuestionId, linkedToQuestionId: questionId),
            }));

            IQuestionnaireStorage questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            interview.Apply(Create.Event.TextListQuestionAnswered(questionId, RosterVector.Empty, new[] { new Tuple<decimal, string>(1, "one"), }));
            eventContext = new EventContext();
        };

        Because of = () => interview.AnswerMultipleOptionsQuestion(userId: userId, questionId: linkedQuestionId, answerTime: answerTime, rosterVector: propagationVector, selectedValues: new [] {1});

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_MultipleOptionsQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<MultipleOptionsQuestionAnswered>();

        It should_raise_MultipleOptionsQuestionAnswered_event_with_QuestionId_equal_to_questionId = () =>
            eventContext.GetSingleEvent<MultipleOptionsQuestionAnswered>().QuestionId.ShouldEqual(linkedQuestionId);

        It should_raise_MultipleOptionsQuestionAnswered_event_with_UserId_equal_to_userId = () =>
            eventContext.GetSingleEvent<MultipleOptionsQuestionAnswered>().UserId.ShouldEqual(userId);

        It should_raise_MultipleOptionsQuestionAnswered_event_with_PropagationVector_equal_to_propagationVector = () =>
            eventContext.GetSingleEvent<MultipleOptionsQuestionAnswered>().RosterVector.ShouldEqual(propagationVector);

        It should_raise_MultipleOptionsQuestionAnswered_event_with_AnswerTime_equal_to_answerTime = () =>
            eventContext.GetSingleEvent<MultipleOptionsQuestionAnswered>().AnswerTimeUtc.Should().BeCloseTo(DateTime.UtcNow, 2000);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid linkedQuestionId = Guid.Parse("A1111111111111111111111111111111");
        private static readonly decimal[] propagationVector = RosterVector.Empty;
        private static DateTime answerTime = 2.August(2010).At(22, 00);
    }
}
