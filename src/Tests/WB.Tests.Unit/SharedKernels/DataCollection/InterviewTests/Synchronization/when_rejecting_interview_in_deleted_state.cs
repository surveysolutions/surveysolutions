using System;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Synchronization
{
    internal class when_rejecting_interview_from_HQ_in_deleted_state : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(Guid.NewGuid(), _ => true);

            interview = CreateInterview(questionnaireRepository: questionnaireRepository);
            interview.Apply(new InterviewStatusChanged(InterviewStatus.Deleted, null, DateTimeOffset.Now));

            eventContext = new EventContext();
            BecauseOf();
        } 

        public void BecauseOf() => interview.RejectInterviewFromHeadquarters(userId, Guid.NewGuid(), Guid.NewGuid(), new InterviewSynchronizationDto(), DateTime.Now);

        [NUnit.Framework.Test] public void should_restore_interview () => eventContext.ShouldContainEvent<InterviewRestored>(@event => @event.UserId == userId);

        static Interview interview;
        static EventContext eventContext;
        static Guid userId = Guid.NewGuid();
    }
}
