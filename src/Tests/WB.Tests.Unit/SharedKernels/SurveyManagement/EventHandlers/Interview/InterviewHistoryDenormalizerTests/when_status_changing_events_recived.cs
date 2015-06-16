using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewHistoryDenormalizerTests
{
    internal class when_status_changing_events_recived : InterviewHistoryDenormalizerTestContext
    {
        Establish context = () =>
        {
            statusEvents = new List<object>();
            statusEvents.Add(new InterviewerAssigned(interviewId, Guid.NewGuid(), DateTime.Now));
            statusEvents.Add(new InterviewCompleted(Guid.NewGuid(), DateTime.Now, "comment"));
            statusEvents.Add(new InterviewRestarted(Guid.NewGuid(), DateTime.Now,"rest"));
            statusEvents.Add(new InterviewRejected(Guid.NewGuid(), "rej", DateTime.Now));
            statusEvents.Add(new InterviewApproved(Guid.NewGuid(), "comment", DateTime.Now));
            statusEvents.Add(new InterviewRestored(Guid.NewGuid()));
            statusEvents.Add(new InterviewRejectedByHQ(Guid.NewGuid(), "rej"));
            statusEvents.Add(new InterviewApprovedByHQ(Guid.NewGuid(), "comment"));
            statusEvents.Add(new InterviewDeleted(interviewId));

            interviewHistoryView = CreateInterviewHistoryView(interviewId);

            interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer();
        };

        Because of = () =>
          PublishEventsOnOnInterviewExportedDataDenormalizer(statusEvents, interviewHistoryView, interviewExportedDataDenormalizer);

        It should_action_of_1_record_be_InterviewerAssigned = () =>
            interviewHistoryView.Records[0].Action.ShouldEqual(InterviewHistoricalAction.InterviewerAssigned);

        It should_action_of_2_record_be_Completed = () =>
          interviewHistoryView.Records[1].Action.ShouldEqual(InterviewHistoricalAction.Completed);

        It should_action_of_3_record_be_Restarted = () =>
          interviewHistoryView.Records[2].Action.ShouldEqual(InterviewHistoricalAction.Restarted);

        It should_action_of_4_record_be_RejectedBySupervisor = () =>
            interviewHistoryView.Records[3].Action.ShouldEqual(InterviewHistoricalAction.RejectedBySupervisor);

        It should_action_of_5_record_be_ApproveBySupervisor = () =>
          interviewHistoryView.Records[4].Action.ShouldEqual(InterviewHistoricalAction.ApproveBySupervisor);

        It should_action_of_6_record_be_Restored = () =>
          interviewHistoryView.Records[5].Action.ShouldEqual(InterviewHistoricalAction.Restored);

        It should_action_of_7_record_be_RejectedByHeadquarter = () =>
           interviewHistoryView.Records[6].Action.ShouldEqual(InterviewHistoricalAction.RejectedByHeadquarter);

        It should_action_of_8_record_be_ApproveByHeadquarter = () =>
          interviewHistoryView.Records[7].Action.ShouldEqual(InterviewHistoricalAction.ApproveByHeadquarter);

        It should_action_of_9_record_be_Deleted = () =>
          interviewHistoryView.Records[8].Action.ShouldEqual(InterviewHistoricalAction.Deleted);


        private static InterviewHistoryDenormalizer interviewExportedDataDenormalizer;
        private static Guid interviewId = Guid.NewGuid();
        private static InterviewHistoryView interviewHistoryView;
        private static List<object> statusEvents;
    }
}
