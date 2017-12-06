using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.StatusChangeHistoryDenormalizerFunctionalTests
{
    [TestFixture]
    internal class StatusChangeHistoryDenormalizerFunctionalTests : StatusChangeHistoryDenormalizerFunctionalTestContext
    {
        [Test]
        public void when_create_interview_status_history_should_contains_timestaps_from_events()
        {
            var interviewId = Guid.NewGuid();
            var currentTime = DateTime.UtcNow;
            var interviewSummary = Create.Entity.InterviewSummary(interviewId);
            var denormalizer = CreateStatusChangeHistoryDenormalizerFunctional();

            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewCreated(interviewId: interviewId, createTime: currentTime.AddSeconds(1)));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.SupervisorAssigned(interviewId: interviewId, assignTime: currentTime.AddSeconds(2)));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewerAssigned(interviewId: interviewId, assignTime: currentTime.AddSeconds(3)));

            var statuses = interviewSummary.InterviewCommentedStatuses;
            Assert.AreEqual(statuses[0].Status, InterviewExportedAction.Created);
            Assert.AreEqual(statuses[0].Timestamp, currentTime.AddSeconds(1));
            Assert.AreEqual(statuses[1].Status, InterviewExportedAction.SupervisorAssigned);
            Assert.AreEqual(statuses[1].Timestamp, currentTime.AddSeconds(2));
            Assert.AreEqual(statuses[2].Status, InterviewExportedAction.InterviewerAssigned);
            Assert.AreEqual(statuses[2].Timestamp, currentTime.AddSeconds(3));
        }

        [Test]
        public void when_just_completed_interview_reassigned_to_supervisor_then_status_should_be_completed()
        {
            var interviewId = Guid.NewGuid();
            var interviewSummary = Create.Entity.InterviewSummary(interviewId);
            var denormalizer = CreateStatusChangeHistoryDenormalizerFunctional();

            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewCreated(interviewId: interviewId));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.SupervisorAssigned(interviewId: interviewId));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewerAssigned(interviewId: interviewId));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewCompleted(interviewId));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.SupervisorAssigned(interviewId: interviewId));

            Assert.AreEqual(InterviewExportedAction.Completed,
                interviewSummary.InterviewCommentedStatuses.LastOrDefault().Status);
        }

        [Test]
        public void when_just_completed_interview_reassigned_to_interviewer_then_status_should_be_completed()
        {
            var interviewId = Guid.NewGuid();
            var interviewSummary = Create.Entity.InterviewSummary(interviewId);
            var denormalizer = CreateStatusChangeHistoryDenormalizerFunctional();

            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewCreated(interviewId: interviewId));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.SupervisorAssigned(interviewId: interviewId));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewerAssigned(interviewId: interviewId));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewCompleted(interviewId));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewerAssigned(interviewId: interviewId));

            Assert.AreEqual(InterviewExportedAction.Completed,
                interviewSummary.InterviewCommentedStatuses.LastOrDefault().Status);
        }

        [Test]
        public void when_just_completed_interview_reassigned_to_supervisor_then_only_1_commented_status_should_be_added()
        {
            var interviewId = Guid.NewGuid();
            var interviewSummary = Create.Entity.InterviewSummary(interviewId);
            var denormalizer = CreateStatusChangeHistoryDenormalizerFunctional();

            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewCreated(interviewId: interviewId));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.SupervisorAssigned(interviewId: interviewId));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewerAssigned(interviewId: interviewId));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewCompleted(interviewId));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.SupervisorAssigned(interviewId: interviewId));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewerAssigned(interviewId: interviewId, interviewerId: null));

            Assert.AreEqual(new[]
            {
                InterviewExportedAction.Created,
                InterviewExportedAction.SupervisorAssigned,
                InterviewExportedAction.InterviewerAssigned,
                InterviewExportedAction.Completed,
                InterviewExportedAction.Completed

            }, interviewSummary.InterviewCommentedStatuses.Select(x => x.Status).ToArray());
        }
    }
}