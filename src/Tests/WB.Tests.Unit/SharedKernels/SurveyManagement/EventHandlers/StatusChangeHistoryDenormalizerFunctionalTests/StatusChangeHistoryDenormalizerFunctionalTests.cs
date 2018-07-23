using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
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
            var currentTime = DateTimeOffset.Now;
            var interviewSummary = Create.Entity.InterviewSummary(interviewId);
            var denormalizer = CreateStatusChangeHistoryDenormalizerFunctional();

            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewCreated(interviewId: interviewId, originDate: currentTime.AddSeconds(1)));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.SupervisorAssigned(interviewId: interviewId, originDate: currentTime.AddSeconds(2)));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewerAssigned(interviewId: interviewId, originDate: currentTime.AddSeconds(3)));

            var statuses = interviewSummary.InterviewCommentedStatuses;
            Assert.That(InterviewExportedAction.Created, Is.EqualTo(statuses[0].Status));
            Assert.That(currentTime.AddSeconds(1).UtcDateTime, Is.EqualTo(statuses[0].Timestamp));
            Assert.That(InterviewExportedAction.SupervisorAssigned, Is.EqualTo(statuses[1].Status));
            Assert.That(currentTime.AddSeconds(2).UtcDateTime, Is.EqualTo(statuses[1].Timestamp));
            Assert.That(InterviewExportedAction.InterviewerAssigned, Is.EqualTo(statuses[2].Status));
            Assert.That(currentTime.AddSeconds(3).UtcDateTime, Is.EqualTo(statuses[2].Timestamp));
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

            Assert.That(interviewSummary.InterviewCommentedStatuses.LastOrDefault().Status,
                Is.EqualTo(InterviewExportedAction.Completed));
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

            Assert.That(interviewSummary.InterviewCommentedStatuses.LastOrDefault().Status, Is.EqualTo(InterviewExportedAction.Completed));
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

            Assert.That(interviewSummary.InterviewCommentedStatuses.Select(x => x.Status).ToArray(), Is.EqualTo(new[]
            {
                InterviewExportedAction.Created,
                InterviewExportedAction.SupervisorAssigned,
                InterviewExportedAction.InterviewerAssigned,
                InterviewExportedAction.Completed,
                InterviewExportedAction.Completed
            }));
        }

        [Test]
        public void should_not_include_pause_events_in_duration_calculations()
        {
            Guid interviewId = Id.gA;

            var denormalizer = CreateStatusChangeHistoryDenormalizerFunctional();
            var interviewSummary = Create.Entity.InterviewSummary(interviewId);
            var startDate = new DateTimeOffset(new DateTime(2010, 11, 1));

            // Act
            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewCreated(interviewId: interviewId, originDate: startDate));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewerAssigned(interviewId: interviewId, originDate: startDate));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.TextQuestionAnswered(interviewId: interviewId, originDate: startDate.AddHours(2)));
            denormalizer.Update(interviewSummary, Create.Event.InterviewPaused(startDate.AddHours(2)).ToPublishedEvent(eventSourceId: interviewId));
            denormalizer.Update(interviewSummary, Create.Event.InterviewResumed(startDate.AddHours(2)).ToPublishedEvent(eventSourceId: interviewId));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewCompleted(interviewId: interviewId, originDate: startDate.AddHours(5)));

            // Assert
            Assert.That(interviewSummary.InterviewCommentedStatuses.Last(), 
                Has.Property(nameof(InterviewCommentedStatus.TimeSpanWithPreviousStatus)).EqualTo(TimeSpan.FromHours(3)));
        }

        [Test]
        public void should_not_include_stop_resume_events_in_duration_calculations()
        {
            Guid interviewId = Id.gA;

            var denormalizer = CreateStatusChangeHistoryDenormalizerFunctional();
            var interviewSummary = Create.Entity.InterviewSummary(interviewId);
            var startDate = new DateTimeOffset(new DateTime(2010, 11, 1));

            // Act
            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewCreated(interviewId: interviewId, originDate: startDate));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.SupervisorAssigned(interviewId: interviewId, originDate: startDate.AddHours(2)));
            denormalizer.Update(interviewSummary, Create.Event.InterviewClosedBySupervisor().ToPublishedEvent(eventSourceId: interviewId));
            denormalizer.Update(interviewSummary, Create.Event.InterviewOpenedBySupervisor().ToPublishedEvent(eventSourceId: interviewId));
            denormalizer.Update(interviewSummary, Create.PublishedEvent.InterviewerAssigned(interviewId: interviewId, originDate: startDate.AddHours(5)));

            // Assert
            Assert.That(interviewSummary.InterviewCommentedStatuses.Last(), 
                Has.Property(nameof(InterviewCommentedStatus.TimeSpanWithPreviousStatus)).EqualTo(TimeSpan.FromHours(3)));
        }
    }
}
