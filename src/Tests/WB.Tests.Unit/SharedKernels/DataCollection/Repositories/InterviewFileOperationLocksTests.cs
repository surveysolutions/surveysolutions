using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Tests.Unit.SharedKernels.DataCollection.Repositories
{
    [TestFixture]
    [TestOf(typeof(InterviewFileOperationLocks))]
    public class InterviewFileOperationLocksTests
    {
        [Test]
        public async Task when_lock_is_held_for_same_interview_should_wait_until_release()
        {
            var interviewId = Guid.NewGuid();
            var first = InterviewFileOperationLocks.Get(interviewId);
            await first.WaitAsync();

            var second = InterviewFileOperationLocks.Get(interviewId);
            var secondWaitTask = second.WaitAsync();

            Assert.That(secondWaitTask.IsCompleted, Is.False);

            first.Dispose();

            Assert.That(await CompletesWithin(secondWaitTask, TimeSpan.FromSeconds(1)), Is.True);
            second.Dispose();
        }

        [Test]
        public async Task when_locks_are_for_different_interviews_should_not_block_each_other()
        {
            var first = InterviewFileOperationLocks.Get(Guid.NewGuid());
            await first.WaitAsync();

            var second = InterviewFileOperationLocks.Get(Guid.NewGuid());
            var secondWaitTask = second.WaitAsync();

            Assert.That(await CompletesWithin(secondWaitTask, TimeSpan.FromSeconds(1)), Is.True);

            second.Dispose();
            first.Dispose();
        }

        [Test]
        public async Task when_lock_is_disposed_should_allow_reacquire_for_the_same_interview()
        {
            var interviewId = Guid.NewGuid();
            var first = InterviewFileOperationLocks.Get(interviewId);
            await first.WaitAsync();
            first.Dispose();

            var second = InterviewFileOperationLocks.Get(interviewId);
            var secondWaitTask = second.WaitAsync();

            Assert.That(await CompletesWithin(secondWaitTask, TimeSpan.FromSeconds(1)), Is.True);
            second.Dispose();
        }

        [Test]
        public async Task when_lock_is_acquired_should_not_create_temp_lock_file()
        {
            var interviewId = Guid.NewGuid();
            var lockFilePath = Path.Combine(Path.GetTempPath(), "WB.InterviewFileOperationLocks", $"{interviewId:N}.lck");
            if (File.Exists(lockFilePath))
                File.Delete(lockFilePath);

            var operationLock = InterviewFileOperationLocks.Get(interviewId);
            await operationLock.WaitAsync();
            operationLock.Dispose();

            Assert.That(File.Exists(lockFilePath), Is.False);
        }

        private static async Task<bool> CompletesWithin(Task task, TimeSpan timeout)
        {
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout));
            return completedTask == task;
        }
    }
}
