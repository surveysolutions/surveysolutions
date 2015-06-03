using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
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
            statusEventsToPublish.Add(Create.InterviewerAssignedEvent(interviewId: interviewId));
            statusEventsToPublish.Add(Create.InterviewCompletedEvent(interviewId: interviewId, comment: "comment Completed"));
            statusEventsToPublish.Add(Create.InterviewRejectedEvent(interviewId: interviewId, comment: "comment Rejected"));
            statusEventsToPublish.Add(Create.InterviewApprovedEvent(interviewId: interviewId, comment: "comment Approved"));
            statusEventsToPublish.Add(Create.InterviewRejectedByHQEvent(interviewId: interviewId, comment: "comment RejectedByHQ"));
            statusEventsToPublish.Add(Create.InterviewApprovedByHQEvent(interviewId: interviewId, comment: "comment ApprovedByHQ"));
            statusEventsToPublish.Add(Create.InterviewRestartedEvent(interviewId: interviewId, comment: "comment Restarted"));
            statusEventsToPublish.Add(Create.SupervisorAssignedEvent(interviewId: interviewId));
            statusEventsToPublish.Add(Create.InterviewRestoredEvent(interviewId: interviewId));
            statusEventsToPublish.Add(Create.InterviewDeletedEvent(interviewId: interviewId));
            statusEventsToPublish.Add(Create.InterviewHardDeletedEvent(interviewId: interviewId));

            denormalizer = CreateDenormalizer(interviewStatuses: interviewStatusesStorage);
        };

        Because of =
            () =>
                denormalizer.Handle(statusEventsToPublish, interviewId);

        It should_store_all_Status_changes_and_preserve_the_order =
            () => history.InterviewCommentedStatuses.Select(i => i.Status).ToArray()
                .ShouldEqual(new[]
                {
                    InterviewStatus.InterviewerAssigned, 
                    InterviewStatus.Completed,
                    InterviewStatus.RejectedBySupervisor, 
                    InterviewStatus.ApprovedBySupervisor, 
                    InterviewStatus.RejectedByHeadquarters, 
                    InterviewStatus.ApprovedByHeadquarters,
                    InterviewStatus.Restarted,
                    InterviewStatus.SupervisorAssigned,
                    InterviewStatus.Restored,
                    InterviewStatus.Deleted,
                    InterviewStatus.Deleted
                });

        It should_store_comments_and_preserve_the_order_for_statuses_Completed_Rejected_Approved_RejectedByHQ_Restarted =
           () => history.InterviewCommentedStatuses.Where(s => 
               new[]
               {
                   InterviewStatus.Completed, 
                   InterviewStatus.RejectedBySupervisor, 
                   InterviewStatus.ApprovedBySupervisor, 
                   InterviewStatus.RejectedByHeadquarters,
                   InterviewStatus.Restarted, 
                   InterviewStatus.ApprovedByHeadquarters
               }.Contains(s.Status)).Select(i => i.Comment).ToArray()
               .ShouldEqual(new[]
                {
                    "comment Completed", 
                    "comment Rejected",
                    "comment Approved", 
                    "comment RejectedByHQ", 
                    "comment ApprovedByHQ", 
                    "comment Restarted"
                });

        private static StatusChangeHistoryDenormalizerFunctional denormalizer;
        private static InterviewStatuses history;
        private static List<IPublishableEvent> statusEventsToPublish;
        private static Guid interviewId=Guid.Parse("11111111111111111111111111111111");
    }
}