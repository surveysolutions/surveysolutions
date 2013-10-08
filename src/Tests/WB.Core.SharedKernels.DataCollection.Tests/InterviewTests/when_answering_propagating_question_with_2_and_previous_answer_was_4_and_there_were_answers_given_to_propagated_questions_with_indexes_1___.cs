using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_answering_propagating_question_with_2_and_previous_answer_was_4_and_there_were_answers_given_to_propagated_questions_with_indexes_1_and_2 : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            propagatingQuestionId = Guid.Parse("11111111111111111111111111111111");
            var propagatedGroupId = Guid.Parse("00000000000000003333333333333333");
            propagatedQuestionId = Guid.Parse("22222222222222222222222222222222");

            var questionaire = Mock.Of<IQuestionnaire>(_

                => _.HasQuestion(propagatingQuestionId) == true
                && _.GetQuestionType(propagatingQuestionId) == QuestionType.AutoPropagate
                && _.ShouldQuestionPropagateGroups(propagatingQuestionId) == true
                && _.GetMaxAnswerValueForPropagatingQuestion(propagatingQuestionId) == 100
                && _.GetGroupsPropagatedByQuestion(propagatingQuestionId) == new[] { propagatedGroupId }

                && _.HasGroup(propagatedGroupId) == true
                && _.GetAllUnderlyingQuestions(propagatedGroupId) == new[] { propagatedQuestionId }

                && _.HasQuestion(propagatedQuestionId) == true
                && _.GetQuestionType(propagatedQuestionId) == QuestionType.Text
                && _.GetPropagationLevelForQuestion(propagatedQuestionId) == 1
                && _.GetParentPropagatableGroupsForQuestionStartingFromTop(propagatedQuestionId) == new [] { propagatedGroupId }
                && _.GetParentPropagatableGroupsAndGroupItselfIfPropagatableStartingFromTop(propagatedGroupId) == new[] { propagatedGroupId });


            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            interview = CreateInterview(questionnaireId: questionnaireId);

            interview.AnswerNumericIntegerQuestion(userId, propagatingQuestionId, new int[]{}, DateTime.Now, 4);
            interview.AnswerTextQuestion(userId, propagatedQuestionId, new [] { 1 }, DateTime.Now, "Answer for index 1");
            interview.AnswerTextQuestion(userId, propagatedQuestionId, new [] { 2 }, DateTime.Now, "Answer for index 2");

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerNumericIntegerQuestion(userId, propagatingQuestionId, new int[]{}, DateTime.Now, 2);

        It should_not_raise_AnswerRemoved_event_with_QuestionId_equal_to_propagated_question_and_PropagationVector_equal_to_0 = () =>
            eventContext.ShouldNotContainEvent<AnswerRemoved>(@event
                => @event.QuestionId == propagatedQuestionId
                && @event.PropagationVector.SequenceEqual(new[] { 0 }));

        It should_not_raise_AnswerRemoved_event_with_QuestionId_equal_to_propagated_question_and_PropagationVector_equal_to_1 = () =>
            eventContext.ShouldNotContainEvent<AnswerRemoved>(@event
                => @event.QuestionId == propagatedQuestionId
                && @event.PropagationVector.SequenceEqual(new[] { 1 }));

        It should_raise_AnswerRemoved_event_with_QuestionId_equal_to_propagated_question_and_PropagationVector_equal_to_2 = () =>
            eventContext.ShouldContainEvent<AnswerRemoved>(@event
                => @event.QuestionId == propagatedQuestionId
                && @event.PropagationVector.SequenceEqual(new[] { 2 }));

        It should_not_raise_AnswerRemoved_event_with_QuestionId_equal_to_propagated_question_and_PropagationVector_equal_to_3 = () =>
            eventContext.ShouldNotContainEvent<AnswerRemoved>(@event
                => @event.QuestionId == propagatedQuestionId
                && @event.PropagationVector.SequenceEqual(new[] { 3 }));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static EventContext eventContext;
        private static Guid propagatedQuestionId;
        private static Interview interview;
        private static Guid userId;
        private static Guid propagatingQuestionId;
    }
}