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
    internal class StatefulInterview_PauseResumeTests
    {
        [Test]
        public void when_pause_command_called_Should_record_pause_date()
        {
            var interview = Create.AggregateRoot.StatefulInterview(Id.gA);

            var dateTimeOffset = new DateTimeOffset(2010, 1, 20, 1, 1, 1, new TimeSpan());
            
            // Act
            using var events = new EventContext(); 
            interview.Pause(Create.Command.PauseInterview(Id.gA, dateTimeOffset));
            
            // Assert
            events.ShouldContainEvent<InterviewPaused>(x => x.UtcTime == dateTimeOffset.UtcDateTime);
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
            events.ShouldContainEvent<InterviewPaused>(x => x.UtcTime == expectedCloseSessionDate);
            events.ShouldContainEvent<InterviewResumed>(x => x.UtcTime == dateTimeOffset5SecondsAfter.UtcDateTime);
        }
        
        [Test]
        public void when_resume_command_arrives_and_interview_has_no_pause_event_and_has_answers_Should_record_PauseEvent_with_time_of_last_answer()
        {
            var questionId = Id.g1;
            var interviewId = Id.gA;
           
            var firstResume = new DateTimeOffset(2010, 1, 20, 1, 1, 1, new TimeSpan());
            var lastAnswerDate = firstResume.AddDays(5);
            var secondResume = firstResume.AddYears(1);

            var clock = Mock.Of<Ncqrs.IClock>(c => c.OffsetNow() == lastAnswerDate);
            
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
            
            events.ShouldContainEvent<InterviewPaused>(x => x.UtcTime == expectedPauseDate);
            events.ShouldContainEvent<InterviewResumed>(x => x.UtcTime == secondResume.UtcDateTime);
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
            events.ShouldContainEvent<InterviewResumed>(x => x.UtcTime == resumeDate1.UtcDateTime);
        }
    }
}
