using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Invitations
{
    [TestFixture]
    public class CompletedEmailsQueueTests
    {
        [TestCase(0, 10)]
        [TestCase(1, 20)]
        [TestCase(2, 30)]
        [TestCase(3, 60)]
        [TestCase(4, 180)]
        [TestCase(20, 180)]
        public void should_schedule_retry_from_failure_time(int previousFailures, int delayMinutes)
        {
            var record = new CompletedEmailRecord
            {
                InterviewId = Guid.NewGuid(), RequestTime = DateTime.UtcNow.AddDays(-10), FailedCount = previousFailures
            };
            var queue = CreateQueue(record);
            var before = DateTime.UtcNow;
            queue.MarkAsFailedToSend(record.InterviewId);
            var after = DateTime.UtcNow;

            Assert.That(record.FailedCount, Is.EqualTo(previousFailures + 1));
            Assert.That(record.NextAttemptAt, Is.InRange(before.AddMinutes(delayMinutes), after.AddMinutes(delayMinutes)));
            Assert.That(queue.GetInterviewIdsForSend(), Is.Empty);

            record.NextAttemptAt = DateTime.UtcNow.AddSeconds(-1);
            Assert.That(queue.GetInterviewIdsForSend(), Is.EqualTo(new[] { record.InterviewId }));
        }

        [Test]
        public void should_recover_legacy_failures_without_a_next_attempt()
        {
            var record = new CompletedEmailRecord
            {
                InterviewId = Guid.NewGuid(), RequestTime = DateTime.UtcNow.AddYears(-1), FailedCount = 10
            };
            Assert.That(CreateQueue(record).GetInterviewIdsForSend(), Does.Contain(record.InterviewId));
        }

        [Test]
        public void failed_first_batch_should_not_block_later_records()
        {
            var records = Enumerable.Range(0, 101).Select(i => new CompletedEmailRecord
            {
                InterviewId = Guid.NewGuid(), RequestTime = DateTime.UtcNow.AddDays(-1).AddSeconds(i)
            }).ToArray();
            var queue = CreateQueue(records);
            var firstBatch = queue.GetInterviewIdsForSend();
            Assert.That(firstBatch.Count, Is.EqualTo(100));
            foreach (var id in firstBatch)
                queue.MarkAsFailedToSend(id);

            Assert.That(queue.GetInterviewIdsForSend(), Is.EqualTo(new[] { records[100].InterviewId }));
        }

        [Test]
        public void missing_record_should_not_fail_failure_acknowledgement()
        {
            Assert.DoesNotThrow(() => CreateQueue().MarkAsFailedToSend(Guid.NewGuid()));
        }

        private static CompletedEmailsQueue CreateQueue(params CompletedEmailRecord[] records) => new(
            new TestPlainStorage<CompletedEmailRecord>(records.ToDictionary(r => (object)r.InterviewId, r => r)));
    }
}

