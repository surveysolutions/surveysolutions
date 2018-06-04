using System;
using System.Collections.Generic;
using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewHistoryDenormalizerTests
{
    internal class when_status_changing_events_recived : InterviewHistoryDenormalizerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            statusEvents = new List<IEvent>();
            statusEvents.Add(new InterviewerAssigned(interviewId, Guid.NewGuid(), originDate: DateTimeOffset.Now));
            statusEvents.Add(new InterviewReceivedByInterviewer(originDate: DateTimeOffset.Now));
            statusEvents.Add(new InterviewCompleted(Guid.NewGuid(), DateTime.Now, "comment"));
            statusEvents.Add(new InterviewRestarted(Guid.NewGuid(), DateTime.Now,"rest"));
            statusEvents.Add(new InterviewReceivedBySupervisor(originDate: DateTimeOffset.Now));
            statusEvents.Add(new InterviewRejected(Guid.NewGuid(), "rej", DateTime.Now));
            statusEvents.Add(new InterviewApproved(Guid.NewGuid(), "comment", DateTime.Now));
            statusEvents.Add(new InterviewRestored(Guid.NewGuid(), originDate: DateTimeOffset.Now));
            statusEvents.Add(new InterviewRejectedByHQ(Guid.NewGuid(), "rej", originDate: DateTimeOffset.Now));
            statusEvents.Add(new InterviewApprovedByHQ(Guid.NewGuid(), "comment", originDate: DateTimeOffset.Now));
            statusEvents.Add(new UnapprovedByHeadquarters(Guid.NewGuid(), "comment", originDate: DateTimeOffset.Now));
            statusEvents.Add(new InterviewApprovedByHQ(Guid.NewGuid(), "comment", originDate: DateTimeOffset.Now));
            statusEvents.Add(new InterviewDeleted(interviewId, originDate: DateTimeOffset.Now));

            interviewHistoryView = CreateInterviewHistoryView(interviewId);

            interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer();
            BecauseOf();
        }

        public void BecauseOf() =>
          PublishEventsOnOnInterviewExportedDataDenormalizer(statusEvents, interviewHistoryView, interviewExportedDataDenormalizer);

        [NUnit.Framework.Test] public void should_action_of_1_record_be_InterviewerAssigned () =>
            interviewHistoryView.Records[0].Action.Should().Be(InterviewHistoricalAction.InterviewerAssigned);

        [NUnit.Framework.Test] public void should_action_of_2_record_be_ReceivedByInterviewer () =>
            interviewHistoryView.Records[1].Action.Should().Be(InterviewHistoricalAction.ReceivedByInterviewer);

        [NUnit.Framework.Test] public void should_action_of_3_record_be_Completed () =>
          interviewHistoryView.Records[2].Action.Should().Be(InterviewHistoricalAction.Completed);

        [NUnit.Framework.Test] public void should_action_of_4_record_be_Restarted () =>
          interviewHistoryView.Records[3].Action.Should().Be(InterviewHistoricalAction.Restarted);

        [NUnit.Framework.Test] public void should_action_of_5_record_be_ReceivedBySupervisor () =>
          interviewHistoryView.Records[4].Action.Should().Be(InterviewHistoricalAction.ReceivedBySupervisor);

        [NUnit.Framework.Test] public void should_action_of_6_record_be_RejectedBySupervisor () =>
            interviewHistoryView.Records[5].Action.Should().Be(InterviewHistoricalAction.RejectedBySupervisor);

        [NUnit.Framework.Test] public void should_action_of_7_record_be_ApproveBySupervisor () =>
          interviewHistoryView.Records[6].Action.Should().Be(InterviewHistoricalAction.ApproveBySupervisor);

        [NUnit.Framework.Test] public void should_action_of_8_record_be_Restored () =>
          interviewHistoryView.Records[7].Action.Should().Be(InterviewHistoricalAction.Restored);

        [NUnit.Framework.Test] public void should_action_of_9_record_be_RejectedByHeadquarter () =>
           interviewHistoryView.Records[8].Action.Should().Be(InterviewHistoricalAction.RejectedByHeadquarter);

        [NUnit.Framework.Test] public void should_action_of_10_record_be_ApproveByHeadquarter () =>
          interviewHistoryView.Records[9].Action.Should().Be(InterviewHistoricalAction.ApproveByHeadquarter);
        
        [NUnit.Framework.Test] public void should_action_of_11_record_be_ApproveByHeadquarter () =>
          interviewHistoryView.Records[10].Action.Should().Be(InterviewHistoricalAction.UnapproveByHeadquarters);

        [NUnit.Framework.Test] public void should_action_of_12_record_be_ApproveByHeadquarter () =>
          interviewHistoryView.Records[11].Action.Should().Be(InterviewHistoricalAction.ApproveByHeadquarter);

        [NUnit.Framework.Test] public void should_action_of_13_record_be_Deleted () =>
          interviewHistoryView.Records[12].Action.Should().Be(InterviewHistoricalAction.Deleted);


        private static InterviewParaDataEventHandler interviewExportedDataDenormalizer;
        private static Guid interviewId = Guid.NewGuid();
        private static InterviewHistoryView interviewHistoryView;
        private static List<IEvent> statusEvents;
    }
}
