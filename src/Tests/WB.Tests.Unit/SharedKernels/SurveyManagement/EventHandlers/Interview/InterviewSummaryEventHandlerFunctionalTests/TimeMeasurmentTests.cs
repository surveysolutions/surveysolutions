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
        public void when_interview_is_paused_with_origin_date_earlier_than_last_resume_due_to_clock_skew_Should_not_reduce_recorded_duration()
        {
            var denormalizer = CreateDenormalizer();
            var summary = Create.Entity.InterviewSummary();

            // Act
            var resumeTime = new DateTimeOffset(new DateTime(2010, 1, 1, 12, 0, 0));
            var updatedModel = denormalizer.Update(summary, Create.Event.InterviewResumed(originDate: resumeTime).ToPublishedEvent());
            // Paused event arrives with an origin date earlier than the resume one, e.g. due to clock skew
            // between devices. This should not make the recorded duration negative/shorter.
            updatedModel = denormalizer.Update(updatedModel, Create.Event.InterviewPaused(originDate: resumeTime.AddMinutes(-30)).ToPublishedEvent());

            // Assert
            Assert.That(updatedModel, Has.Property(nameof(updatedModel.LastResumeEventUtcTimestamp)).Null);
            Assert.That(updatedModel, Has.Property(nameof(updatedModel.InterviewDuration)).EqualTo(TimeSpan.Zero));
        }
    }
}
