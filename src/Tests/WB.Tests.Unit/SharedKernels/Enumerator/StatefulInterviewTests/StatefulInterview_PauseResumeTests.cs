using System;
using System.Linq;
using Moq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    [TestOf(typeof(StatefulInterview))]
    public class StatefulInterview_PauseResumeTests
    {
        [Test]
        public void when_second_resume_arrives_within_30_seconds_from_last_resume_Should_not_raise_any_events()
        {
            var interview = Create.AggregateRoot.StatefulInterview(Id.gA);

            var dateTimeOffset = new DateTimeOffset(2010, 1, 20, 1, 1, 1, new TimeSpan());
            
            interview.Resume(Create.Command.ResumeInterview(Id.gA, dateTimeOffset));
            // Act
            using var events = new EventContext(); 
            interview.Resume(Create.Command.ResumeInterview(Id.gA, dateTimeOffset.AddSeconds(10)));
            
            // Assert
            events.ShouldNotContainEvent<InterviewPaused>();
            events.ShouldNotContainEvent<InterviewResumed>();
        }

        [Test]
        public void when_pause_occurs_within_quite_window_Should_not_publish_event()
        {
            var interview = Create.AggregateRoot.StatefulInterview(Id.gA);

            var dateTimeOffset = new DateTimeOffset(2010, 1, 20, 1, 1, 1, new TimeSpan());
            
            interview.Resume(Create.Command.ResumeInterview(Id.gA, dateTimeOffset));
            // Act
            using var events = new EventContext(); 
            interview.Pause(Create.Command.PauseInterview(Id.gA, dateTimeOffset.AddSeconds(10)));
            
            // Assert
            events.ShouldNotContainEvent<InterviewPaused>();
            events.ShouldNotContainEvent<InterviewResumed>();
        }

        [Test]
        public void when_interview_is_completed_Should_raise_interview_paused()
        {
            var interview = Create.AggregateRoot.StatefulInterview(Id.gA);

            var dateTimeOffset = new DateTimeOffset(2010, 1, 20, 1, 1, 1, new TimeSpan());
            
            // Act
            using var events = new EventContext(); 
            interview.Complete(Id.gA, "", dateTimeOffset.AddSeconds(10));
            
            // Assert
            events.ShouldContainEvent<InterviewPaused>();
        }
        
        [Test]
        public void when_pause_occurs_outside_of_quite_window_Should_raise_pause_event()
        {
            var interview = Create.AggregateRoot.StatefulInterview(Id.gA);

            var dateTimeOffset = new DateTimeOffset(2010, 1, 20, 1, 1, 1, new TimeSpan());
            
            interview.Resume(Create.Command.ResumeInterview(Id.gA, dateTimeOffset));
            // Act
            using var events = new EventContext();
            var pauseDate = dateTimeOffset.AddMinutes(1).AddMilliseconds(1);
            interview.Pause(Create.Command.PauseInterview(Id.gA, pauseDate));
            
            // Assert
            events.ShouldContainEvent<InterviewPaused>(x => x.OriginDate.Value.UtcDateTime == pauseDate.UtcDateTime);
        }
        
        [Test]
        public void when_resume_command_arrives_and_interview_has_no_pause_event_and_has_no_answers_Should_record_PauseEvent_in_15_minutes_after_last_resume()
        {
            var interview = Create.AggregateRoot.StatefulInterview(Id.gA);

            var dateTimeOffset = new DateTimeOffset(2010, 1, 20, 1, 1, 1, new TimeSpan());
            var dateTimeOffset5SecondsAfter = dateTimeOffset.AddYears(1);
            
            // Act
            interview.Resume(Create.Command.ResumeInterview(Id.gA, dateTimeOffset));
            
            using var events = new EventContext(); 
            interview.Resume(Create.Command.ResumeInterview(Id.gA, dateTimeOffset5SecondsAfter));
            
            // Assert
            var expectedCloseSessionDate = dateTimeOffset.UtcDateTime.AddMinutes(15);
            events.ShouldContainEvent<InterviewPaused>(x => x.OriginDate.Value.UtcDateTime == expectedCloseSessionDate);
            events.ShouldContainEvent<InterviewResumed>(x => x.OriginDate.Value.UtcDateTime == dateTimeOffset5SecondsAfter.UtcDateTime);
        }

        [Test]
        public void when_resume_command_arrives_and_interview_has_no_pause_event_and_has_answers_Should_record_PauseEvent_with_time_of_last_answer()
        {
            var questionId = Id.g1;
            var interviewId = Id.gA;
           
            var firstResume = new DateTimeOffset(2010, 1, 20, 1, 1, 1, new TimeSpan());
            var lastAnswerDate = firstResume.AddDays(5);
            var secondResume = firstResume.AddYears(1);

            var clock = Mock.Of<Ncqrs.IClock>(c => c.DateTimeOffsetNow() == lastAnswerDate);
            
            var interview = Create.AggregateRoot.StatefulInterview(interviewId: interviewId,
                questionnaire: Create.Entity.QuestionnaireDocumentWithOneQuestion(questionId),
                clock: clock);
            
            // Act
            interview.Resume(Create.Command.ResumeInterview(interviewId, firstResume));
            interview.AnswerTextQuestion(Id.gC, questionId, RosterVector.Empty, lastAnswerDate, "text");
            
            using var events = new EventContext(); 
            interview.Resume(Create.Command.ResumeInterview(interviewId, secondResume));
            
            // Assert
            DateTime expectedPauseDate = lastAnswerDate.UtcDateTime;
            
            events.ShouldContainEvent<InterviewPaused>(x => x.OriginDate.Value.UtcDateTime == expectedPauseDate);
            events.ShouldContainEvent<InterviewResumed>(x => x.OriginDate.Value.UtcDateTime == secondResume.UtcDateTime);
        }

        [Test]
        public void when_last_session_closed_should_just_raise_resume_event()
        {
            var interview = Create.AggregateRoot.StatefulInterview(Id.gA);

            var resumeDate = new DateTimeOffset(2010, 1, 20, 1, 1, 1, new TimeSpan());
            var pauseDate = resumeDate.AddHours(1);
            var resumeDate1 = resumeDate.AddHours(1);
            
            // Act
            interview.Resume(Create.Command.ResumeInterview(Id.gA, resumeDate));
            interview.Pause(Create.Command.PauseInterview(Id.gA, pauseDate));
            
            using var events = new EventContext(); 
            interview.Resume(Create.Command.ResumeInterview(Id.gA, resumeDate1));
            
            // Assert
            Assert.That(events.Events.Count(), Is.EqualTo(1));
            events.ShouldContainEvent<InterviewResumed>(x => x.OriginDate.Value.UtcDateTime == resumeDate1.UtcDateTime);
        }

        [Test]
        public void when_pause_arrives_on_non_resumed_interview_Should_not_raise_pause_event()
        {
            var interview = Create.AggregateRoot.StatefulInterview(Id.gA);
            
            // Act
            using var events = new EventContext(); 
            interview.Pause(Create.Command.PauseInterview(Id.gA, new DateTimeOffset()));
            
            // Assert
            Assert.That(events.Events, Is.Empty);
        }
    }
}
