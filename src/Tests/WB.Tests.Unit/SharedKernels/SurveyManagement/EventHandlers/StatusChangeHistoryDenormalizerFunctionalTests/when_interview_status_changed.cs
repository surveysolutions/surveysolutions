using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.StatusChangeHistoryDenormalizerFunctionalTests
{
    internal class when_interview_status_changed : StatusChangeHistoryDenormalizerFunctionalTestContext
    {
        Establish context = () =>
        {
            history = new InterviewStatuses() { InterviewId = interviewId.FormatGuid() };

            var interviewStatusesStorage = new TestInMemoryWriter<InterviewStatuses>();
            interviewStatusesStorage.Store(history, history.InterviewId);

            statusEventsToPublish = new List<IPublishableEvent>();
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewerAssignedEvent(interviewId: interviewId));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewCompletedEvent(interviewId: interviewId, comment: "comment Completed"));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewRejectedEvent(interviewId: interviewId, comment: "comment Rejected"));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewApprovedEvent(interviewId: interviewId, comment: "comment Approved"));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewRejectedByHQEvent(interviewId: interviewId, comment: "comment RejectedByHQ"));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewApprovedByHQEvent(interviewId: interviewId, comment: "comment ApprovedByHQ"));
                                             
            statusEventsToPublish.Add(Create.PublishedEvent.UnapprovedByHeadquartersEvent(interviewId: interviewId, comment: "comment Unapproved"));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewApprovedByHQEvent(interviewId: interviewId, comment: "comment ApprovedByHQ 2nd"));
                                             
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewRestartedEvent(interviewId: interviewId, comment: "comment Restarted"));
            statusEventsToPublish.Add(Create.PublishedEvent.SupervisorAssignedEvent(interviewId: interviewId));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewRestoredEvent(interviewId: interviewId));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewDeletedEvent(interviewId: interviewId));
            statusEventsToPublish.Add(Create.PublishedEvent.InterviewHardDeletedEvent(interviewId: interviewId));

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
                    InterviewExportedAction.Deleted,
                    InterviewExportedAction.Deleted
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
        private static InterviewStatuses history;
        private static List<IPublishableEvent> statusEventsToPublish;
        private static Guid interviewId=Guid.Parse("11111111111111111111111111111111");
    }
}