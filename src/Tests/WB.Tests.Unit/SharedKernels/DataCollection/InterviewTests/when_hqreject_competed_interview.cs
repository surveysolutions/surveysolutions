using System;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [TestFixture]
    [TestOf(typeof(Interview))]
    internal class when_hqreject_competed_interview : InterviewTestsContext
    {
        [Test]
        public void should_raise_InterviewApprovedByHQ_event()
        {
            // arrange
            interview = Create.AggregateRoot.StatefulInterview(questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter());
            interview.AssignSupervisor(hqId, supervisorId, DateTime.Now);
            interview.AssignInterviewer(supervisorId, interId, DateTime.Now);
            interview.Complete(interId, null, DateTime.Now);

            eventContext = new EventContext();
            //act
            interview.HqReject(hqId, string.Empty, DateTimeOffset.Now);

            //assert
            eventContext.ShouldContainEvent<InterviewRejected>(@event => @event.UserId == hqId);
            eventContext.ShouldContainEvent<InterviewStatusChanged>(@event => @event.Status == InterviewStatus.RejectedBySupervisor);
        }

        private static Guid interId = Id.g1;
        private static Guid supervisorId = Id.g2;
        private static Guid hqId = Id.g3;

        private static Guid questionnaireId = Id.gA;
        private static EventContext eventContext;
        private static StatefulInterview interview;
    }
}
