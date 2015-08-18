using System;

using Machine.Specifications;

using Main.Core.Entities.SubEntities;

using Ncqrs.Spec;

using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.DataTransferObjects;
using WB.Core.SharedKernels.Enumerator.Events;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Aggregates;

namespace WB.Tests.Unit.BoundedContexts.Tester.Aggregates
{
    internal class when_restoring_interview_state_from_sync_package_with_linked_single_question : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            Setup.QuestionnaireWithRepositoryToMockedServiceLocator(questionnaireId, _
                => _.GetQuestionType(questionId) == QuestionType.SingleOption
                   && _.IsQuestionLinked(questionId) == true
                   && _.IsQuestionInteger(questionId) == false);

            interview = Create.StatefulInterview(questionnaireId: questionnaireId);

            var answersDtos = new[]
                              {
                                  CreateAnsweredQuestionSynchronizationDto(questionId, rosterVector, answer)
                              };

            synchronizationDto = Create.InterviewSynchronizationDto(questionnaireId, userId, answersDtos);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            interview.RestoreInterviewStateFromSyncPackage(userId, synchronizationDto);

        It should_rise_InterviewSynchronized_event = () =>
            eventContext.ShouldContainEvent<InterviewSynchronized>();

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_correct_userId = () =>
            eventContext.ShouldContainEvent<InterviewAnswersFromSyncPackageRestored>(x => x.UserId == userId);

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_and_other_fields = () =>
        {
            var answerDto = eventContext.GetSingleEvent<InterviewAnswersFromSyncPackageRestored>().Answers[0];
            answerDto.Type.ShouldEqual(AnswerType.RosterVector);
            answerDto.Id.ShouldEqual(questionId);
            answerDto.RosterVector.ShouldEqual(rosterVector);
            answerDto.Answer.ShouldEqual(answer);
        };

        static readonly object answer = new decimal[]{ 1m };
        private static EventContext eventContext;
        private static InterviewSynchronizationDto synchronizationDto;
        private static StatefulInterview interview;
        private static readonly Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid questionId = Guid.Parse("44444444444444444444444444444444");
        private static readonly decimal[] rosterVector = new decimal[] { 1m, 0m };
        private static readonly Guid userId = Guid.Parse("99999999999999999999999999999999");
    }
}