using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.StatusChangeHistoryDenormalizerFunctionalTests
{
    internal class when_interview_status_changed : StatusChangeHistoryDenormalizerFunctionalTestContext
    {
        Establish context = () =>
        {
            history = new InterviewSummary() { SummaryId = interviewId.FormatGuid() };

            var interviewStatusesStorage = new TestInMemoryWriter<InterviewSummary>();
            interviewStatusesStorage.Store(history, history.SummaryId);

            statusEventsToPublish = new List<IPublishableEvent>();
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewerAssigned(interviewId: interviewId));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewCompleted(interviewId: interviewId, comment: "comment Completed"));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewRejected(interviewId: interviewId, comment: "comment Rejected"));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewApproved(interviewId: interviewId, comment: "comment Approved"));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewRejectedByHQ(interviewId: interviewId, comment: "comment RejectedByHQ"));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewApprovedByHQ(interviewId: interviewId, comment: "comment ApprovedByHQ"));
                                             
            statusEventsToPublish.Add(Create.PublishedEvent.UnapprovedByHeadquarters(interviewId: interviewId, comment: "comment Unapproved"));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewApprovedByHQ(interviewId: interviewId, comment: "comment ApprovedByHQ 2nd"));
                                             
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewRestarted(interviewId: interviewId, comment: "comment Restarted"));
            statusEventsToPublish.Add(Create.PublishedEvent.SupervisorAssigned(interviewId: interviewId));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewRestored(interviewId: interviewId));
      

            denormalizer = CreateDenormalizer(interviewStatuses: interviewStatusesStorage);
        };

        Because of =
            () =>
                denormalizer.Handle(statusEventsToPublish, interviewId);

        It should_store_all_Status_changes_and_preserve_the_order =
            () => history.InterviewCommentedStatuses.Select(i => i.Status).ToArray()
                .ShouldEqual(new[]
                {
                    InterviewExportedAction.InterviewerAssigned, 
                    InterviewExportedAction.Completed,
                    InterviewExportedAction.RejectedBySupervisor, 
                    InterviewExportedAction.ApprovedBySupervisor, 
                    InterviewExportedAction.RejectedByHeadquarter, 
                    InterviewExportedAction.ApprovedByHeadquarter,
                    InterviewExportedAction.UnapprovedByHeadquarter,
                    InterviewExportedAction.ApprovedByHeadquarter,
                    InterviewExportedAction.Restarted,
                    InterviewExportedAction.SupervisorAssigned,
                    InterviewExportedAction.Restored,
                });

        It should_store_comments_and_preserve_the_order_for_statuses_Completed_Rejected_Approved_RejectedByHQ_Restarted =
           () => history.InterviewCommentedStatuses.Where(s => 
               new[]
               {
                   InterviewExportedAction.Completed, 
                   InterviewExportedAction.RejectedBySupervisor, 
                   InterviewExportedAction.ApprovedBySupervisor, 
                   InterviewExportedAction.RejectedByHeadquarter,
                   InterviewExportedAction.Restarted, 
                   InterviewExportedAction.ApprovedByHeadquarter,
                   InterviewExportedAction.UnapprovedByHeadquarter

               }.Contains(s.Status)).Select(i => i.Comment).ToArray()
               .ShouldEqual(new[]
                {
                    "comment Completed", 
                    "comment Rejected",
                    "comment Approved", 
                    "comment RejectedByHQ", 
                    "comment ApprovedByHQ",
                    "comment Unapproved",
                    "comment ApprovedByHQ 2nd",
                    "comment Restarted"
                });

        private static StatusChangeHistoryDenormalizerFunctional denormalizer;
        private static InterviewSummary history;
        private static List<IPublishableEvent> statusEventsToPublish;
        private static Guid interviewId=Guid.Parse("11111111111111111111111111111111");
    }
}