using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.ServicesIntegration.Export;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.StatusChangeHistoryDenormalizerFunctionalTests
{
    internal class when_interview_status_changed : StatusChangeHistoryDenormalizerFunctionalTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
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
            BecauseOf();
        }

        private void BecauseOf() => denormalizer.Handle(statusEventsToPublish);

        [NUnit.Framework.Test] public void should_store_all_Status_changes_and_preserve_the_order () => 
            history.InterviewCommentedStatuses.Select(i => i.Status).ToArray()
                .Should().BeEquivalentTo(new[]
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

        [NUnit.Framework.Test] public void should_store_comments_and_preserve_the_order_for_statuses_Completed_Rejected_Approved_RejectedByHQ_Restarted () => history.InterviewCommentedStatuses.Where(s => 
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
               .Should().BeEquivalentTo(new[]
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

        private static InterviewSummaryCompositeDenormalizer denormalizer;
        private static InterviewSummary history;
        private static List<IPublishableEvent> statusEventsToPublish;
        private static Guid interviewId=Guid.Parse("11111111111111111111111111111111");
    }
}
