using System;
using System.Collections.Generic;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.PauseResume
{
    [TestFixture]
    internal class ResumeTests : StatefulInterviewTestsContext
    {
        static readonly List<InterviewStatus> AllowedStatuses = new List<InterviewStatus>
        {
            InterviewStatus.InterviewerAssigned,
            InterviewStatus.RejectedBySupervisor
        };

        [Test]
        public void should_reject_all_statuses_except_allowed([Values] InterviewStatus status)
        {
            Guid interviewId = Id.g1;
            var interview = Create.AggregateRoot.StatefulInterview(interviewId: interviewId);
            interview.Apply(new InterviewStatusChanged(status, null, DateTimeOffset.Now));

            if (AllowedStatuses.Contains(status))
            {
                using (var context = new EventContext())
                {
                    interview.Resume(new ResumeInterviewCommand(interviewId, Id.g2));
                    if (AllowedStatuses.Contains(status))
                    {
                        context.ShouldContainEvent<InterviewResumed>();
                    }
                    else
                    {
                        context.ShouldNotContainEvent<InterviewResumed>();
                    }
                }
            }
        }
    }
}
