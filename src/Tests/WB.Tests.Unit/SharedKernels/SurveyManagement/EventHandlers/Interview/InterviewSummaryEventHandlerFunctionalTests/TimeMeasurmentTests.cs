using System;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class TimeMeasurmentTests : InterviewSummaryDenormalizerTests
    {
        [Test]
        public void when_handing_interview_paused_event_after_created_event_Should_update_total_time_spent_on_interview()
        {
            var denormalizer = CreateDenormalizer();

            // Act
            var creationTime = new DateTimeOffset(new DateTime(2010, 1, 1));
            var updatedModel = denormalizer.Update(null, Create.Event.InterviewCreated(originDate: creationTime).ToPublishedEvent());
            updatedModel = denormalizer.Update(updatedModel, Create.Event.InterviewPaused(originDate: creationTime.AddHours(1)).ToPublishedEvent());

            // Assert
            Assert.That(updatedModel, Has.Property(nameof(updatedModel.LastResumeEventUtcTimestamp)).Null);
            Assert.That(updatedModel, Has.Property(nameof(updatedModel.InterviewDuration)).EqualTo(TimeSpan.FromHours(1)));
        }

        [Test]
        public void when_interview_completed_without_pauses_Should_record_total_time_spent()
        {
            var denormalizer = CreateDenormalizer();

            // Act
            var creationTime = new DateTimeOffset(new DateTime(2010, 1, 1));
            var updatedModel = denormalizer.Update(null, Create.Event.InterviewCreated(originDate: creationTime).ToPublishedEvent());
            updatedModel = denormalizer.Update(updatedModel, Create.Event.InterviewStatusChanged(InterviewStatus.Completed, originDate: creationTime.AddHours(1)).ToPublishedEvent());

            // Assert
            Assert.That(updatedModel, Has.Property(nameof(updatedModel.LastResumeEventUtcTimestamp)).Null);
            Assert.That(updatedModel, Has.Property(nameof(updatedModel.InterviewDuration)).EqualTo(TimeSpan.FromHours(1)));
        }

        [Test]
        public void when_interview_is_paused_after_resuming_Should_record_interviewing_time()
        {
            var denormalizer = CreateDenormalizer();
            var summary = Create.Entity.InterviewSummary();

            // Act
            var creationTime = new DateTimeOffset(new DateTime(2010, 1, 1));
            var updatedModel = denormalizer.Update(summary, Create.Event.InterviewResumed(originDate: creationTime).ToPublishedEvent());
            updatedModel = denormalizer.Update(updatedModel, Create.Event.InterviewPaused(originDate: creationTime.AddHours(1)).ToPublishedEvent());

            // Assert
            Assert.That(updatedModel, Has.Property(nameof(updatedModel.LastResumeEventUtcTimestamp)).Null);
            Assert.That(updatedModel, Has.Property(nameof(updatedModel.InterviewDuration)).EqualTo(TimeSpan.FromHours(1)));
        }

        [Test]
        public void when_interview_is_completed_after_resuming_Should_record_interviewing_time()
        {
            var denormalizer = CreateDenormalizer();
            var summary = Create.Entity.InterviewSummary();

            // Act
            var creationTime = new DateTimeOffset(new DateTime(2010, 1, 1));
            var updatedModel = denormalizer.Update(summary, Create.Event.InterviewResumed(originDate: creationTime).ToPublishedEvent());
            updatedModel = denormalizer.Update(updatedModel, Create.Event.InterviewStatusChanged(InterviewStatus.Completed, originDate: creationTime.AddHours(1)).ToPublishedEvent());

            // Assert
            Assert.That(updatedModel, Has.Property(nameof(updatedModel.LastResumeEventUtcTimestamp)).Null);
            Assert.That(updatedModel, Has.Property(nameof(updatedModel.InterviewDuration)).EqualTo(TimeSpan.FromHours(1)));
        }
    }
}
