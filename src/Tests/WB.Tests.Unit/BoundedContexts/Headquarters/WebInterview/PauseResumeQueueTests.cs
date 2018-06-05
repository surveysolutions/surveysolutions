using System;
using Moq;
using Ncqrs;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.BoundedContexts.Headquarters.WebInterview.Impl;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.WebInterview
{
    [TestOf(typeof(PauseResumeQueue))]
    public class PauseResumeQueueTests 
    {
        readonly TrackingSettings trackingSettings = new TrackingSettings(TimeSpan.FromSeconds(5));
        readonly Guid interviewId = Id.g1;

        [Test]
        public void when_pause_command_arrived_just_after_resume_Should_cancel_resume()
        {
            var firstCommandDate = new DateTimeOffset(new DateTime(2007, 10, 17, 10, 0, 0));

            var now = firstCommandDate.Add(trackingSettings.DelayBeforeCommandPublish).AddSeconds(10);
            var clock = Mock.Of<IClock>(x => x.UtcNow() == now.UtcDateTime);
            IPauseResumeQueue queue = CreateQueue(clock);

            // Act
            queue.EnqueueResume(Create.Command.ResumeInterview(interviewId, firstCommandDate));
            queue.EnqueuePause(Create.Command.PauseInterview(interviewId, firstCommandDate.AddSeconds(1)));

            // Assert
            var forPublish = queue.DeQueueForPublish();
            Assert.That(forPublish, Is.Empty);
        }

        [Test]
        public void when_resume_command_survived_delay_It_should_be_returned_for_publishing()
        {
            var commandDate = new DateTime(2007, 10, 17, 10, 0, 0);

            var now = commandDate.Add(trackingSettings.DelayBeforeCommandPublish).AddSeconds(1);
            var clock = Mock.Of<IClock>(x => x.UtcNow() == now);
            IPauseResumeQueue queue = CreateQueue(clock);

            // Act
            var resumeInterviewCommand = Create.Command.ResumeInterview(interviewId, commandDate);
            queue.EnqueueResume(resumeInterviewCommand);

            // Assert
            var forPublish = queue.DeQueueForPublish();
            Assert.That(forPublish, Has.Count.EqualTo(1));
            Assert.That(forPublish[0], Is.EqualTo(resumeInterviewCommand));
        }

        [Test]
        public void when_command_is_waiting_if_it_might_be_cancelled_It_should_not_be_returned_for_publishing_or_deleted()
        {
            var commandDate = new DateTimeOffset(new DateTime(2007, 10, 17, 10, 0, 0));

            var now = commandDate.Add(trackingSettings.DelayBeforeCommandPublish).AddSeconds(-1);
            var clock = new Mock<IClock>();
            clock.Setup(x => x.UtcNow()).Returns(now.UtcDateTime);
            IPauseResumeQueue queue = CreateQueue(clock.Object);

            // Act
            var resumeInterviewCommand = Create.Command.ResumeInterview(interviewId, commandDate);
            queue.EnqueueResume(resumeInterviewCommand);

            // Assert
            var forPublish = queue.DeQueueForPublish();
            Assert.That(forPublish, Is.Empty);

            // Should be returned when it is waited enough
            clock.Setup(x => x.UtcNow()).Returns(commandDate.Add(trackingSettings.DelayBeforeCommandPublish).AddSeconds(1).UtcDateTime);

            var forPublish1 = queue.DeQueueForPublish();
            Assert.That(forPublish1, Has.Count.EqualTo(1));
            Assert.That(forPublish1[0], Is.EqualTo(resumeInterviewCommand));


            Assert.That(queue.DeQueueForPublish(), Is.Empty, "Should be empty after dequeue");
        }

        [Test]
        public void when_pause_and_resume_are_in_same_scope_but_are_on_distant_time_to_be_published()
        {
            var interviewId = Id.gA;

            var startDate = new DateTime(2010, 1, 2);
            var secondCommandDate = startDate.Add(trackingSettings.PauseResumeGracePeriod).AddSeconds(1);

            var pauseCommand = Create.Command.PauseInterview(interviewId, startDate);
            var resumeCommand = Create.Command.ResumeInterview(interviewId, secondCommandDate);

            var clock = Mock.Of<IClock>(x => x.UtcNow() == startDate.Add(trackingSettings.DelayBeforeCommandPublish).AddSeconds(30));

            var pauseResumeQueue = CreateQueue(clock);

            // Act
            pauseResumeQueue.EnqueuePause(pauseCommand);
            pauseResumeQueue.EnqueueResume(resumeCommand);
            var commandsToPublish = pauseResumeQueue.DeQueueForPublish();
            
            // Assert
            Assert.That(commandsToPublish, Has.Count.EqualTo(2));
        }
        
        private IPauseResumeQueue CreateQueue(IClock clock = null)
        {
            return new PauseResumeQueue(trackingSettings, clock);
        }
    }
}
