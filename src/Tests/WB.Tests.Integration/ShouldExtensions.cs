using System;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;

namespace WB.Tests.Integration
{
    internal static class ShouldExtensions
    {
        public static void ShouldContainEvent<TEvent>(this EventContext eventContext, Func<TEvent, bool> condition = null)
        {
            if (condition == null)
            {
                eventContext.Events.ShouldContain(@event
                    => @event.Payload is TEvent);
            }
            else
            {
                eventContext.Events.ShouldContain(@event
                    => @event.Payload is TEvent
                    && condition.Invoke((TEvent) @event.Payload));
            }
        }

        public static void ShouldNotContainEvent<TEvent>(this EventContext eventContext, Func<TEvent, bool> condition = null)
        {
            if (condition == null)
            {
                eventContext.Events.ShouldNotContain(@event
                    => @event.Payload is TEvent);
            }
            else
            {
                eventContext.Events.ShouldNotContain(@event
                    => @event.Payload is TEvent
                    && condition.Invoke((TEvent) @event.Payload));
            }
        }

        public static void ShouldContainValues(this QuestionnaireStatisticsForChart stat,
            int createdCount,
            int supervisorAssignedCount,
            int interviewerAssignedCount,
            int completedCount,
            int approvedBySupervisorCount,
            int rejectedBySupervisorCount,
            int approvedByHeadquartersCount,
            int rejectedByHeadquartersCount)
        {
            stat.SupervisorAssignedCount.ShouldEqual(supervisorAssignedCount);
            stat.InterviewerAssignedCount.ShouldEqual(interviewerAssignedCount);
            stat.CompletedCount.ShouldEqual(completedCount);
            stat.ApprovedBySupervisorCount.ShouldEqual(approvedBySupervisorCount);
            stat.RejectedBySupervisorCount.ShouldEqual(rejectedBySupervisorCount);
            stat.ApprovedByHeadquartersCount.ShouldEqual(approvedByHeadquartersCount);
            stat.RejectedByHeadquartersCount.ShouldEqual(rejectedByHeadquartersCount);
        }
    }
}