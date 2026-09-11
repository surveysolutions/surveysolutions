using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Storage
{
    [TestFixture]
    public class InterviewS3FileStorageTests
    {
        private Mock<IExternalFileStorage> externalFileStorage;
        private ImageInterviewS3FileStorage storage;

        [SetUp]
        public void SetUp()
        {
            this.externalFileStorage = new Mock<IExternalFileStorage>();
            this.storage = new ImageInterviewS3FileStorage(this.externalFileStorage.Object,
                Mock.Of<IFileSystemAccessor>());
        }

        [Test]
        public async Task should_remove_all_objects_stored_under_interview_prefix()
        {
            var interviewId = Guid.NewGuid();
            var prefix = $"images/{interviewId.FormatGuid()}/";

            this.externalFileStorage.SetupSequence(s => s.ListAsync(prefix))
                .ReturnsAsync(new List<FileObject>
                {
                    new FileObject { Path = prefix + "1.jpg" },
                    new FileObject { Path = prefix + "2.jpg" }
                })
                .ReturnsAsync(new List<FileObject>());

            await this.storage.RemoveAllBinaryDataForInterviewsAsync(new List<Guid> { interviewId });

            this.externalFileStorage.Verify(s => s.RemoveAsync(
                It.Is<IEnumerable<string>>(paths => paths.SequenceEqual(new[] { prefix + "1.jpg", prefix + "2.jpg" }))),
                Times.Once);
        }

        [Test]
        public async Task should_not_remove_anything_when_no_files_stored_for_interview()
        {
            var interviewId = Guid.NewGuid();

            this.externalFileStorage.Setup(s => s.ListAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<FileObject>());

            await this.storage.RemoveAllBinaryDataForInterviewsAsync(new List<Guid> { interviewId });

            this.externalFileStorage.Verify(s => s.RemoveAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        [Test]
        public async Task should_not_fail_when_last_allowed_batch_removes_all_objects()
        {
            var interviewId = Guid.NewGuid();
            var prefix = $"images/{interviewId.FormatGuid()}/";
            var listCallsCount = 0;

            this.externalFileStorage.Setup(s => s.ListAsync(prefix))
                .ReturnsAsync(() => ++listCallsCount > MaxDeletionBatches
                    ? new List<FileObject>()
                    : new List<FileObject> { new FileObject { Path = prefix + listCallsCount } });

            await this.storage.RemoveAllBinaryDataForInterviewsAsync(new List<Guid> { interviewId });

            this.externalFileStorage.Verify(s => s.RemoveAsync(It.IsAny<IEnumerable<string>>()),
                Times.Exactly(MaxDeletionBatches));
        }

        [Test]
        public void should_fail_when_objects_are_still_stored_under_prefix_after_all_batches()
        {
            var interviewId = Guid.NewGuid();
            var prefix = $"images/{interviewId.FormatGuid()}/";

            this.externalFileStorage.Setup(s => s.ListAsync(prefix))
                .ReturnsAsync(() => new List<FileObject> { new FileObject { Path = prefix + "1.jpg" } });

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                this.storage.RemoveAllBinaryDataForInterviewsAsync(new List<Guid> { interviewId }));
        }

        [Test]
        public async Task should_remove_interview_prefixes_with_bounded_concurrency()
        {
            const int interviewsCount = 20;
            var interviewIds = Enumerable.Range(0, interviewsCount).Select(_ => Guid.NewGuid()).ToList();
            var running = 0;
            var maxRunning = 0;
            var allowToComplete = new TaskCompletionSource<bool>();

            this.externalFileStorage.Setup(s => s.ListAsync(It.IsAny<string>()))
                .Returns(async () =>
                {
                    var current = Interlocked.Increment(ref running);
                    InterlockedExtensions.SetMax(ref maxRunning, current);

                    if (current >= MaxParallelPrefixDeletions)
                        allowToComplete.TrySetResult(true);

                    await Task.WhenAny(allowToComplete.Task, Task.Delay(TimeSpan.FromSeconds(5)));

                    Interlocked.Decrement(ref running);

                    return new List<FileObject>();
                });

            await this.storage.RemoveAllBinaryDataForInterviewsAsync(interviewIds);

            Assert.That(maxRunning, Is.EqualTo(MaxParallelPrefixDeletions));
            this.externalFileStorage.Verify(s => s.ListAsync(It.IsAny<string>()), Times.Exactly(interviewsCount));
        }

        private const int MaxDeletionBatches = 1000;
        private const int MaxParallelPrefixDeletions = 8;

        private static class InterlockedExtensions
        {
            public static void SetMax(ref int target, int value)
            {
                int current;

                while (value > (current = Volatile.Read(ref target)))
                {
                    if (Interlocked.CompareExchange(ref target, value, current) == current)
                        return;
                }
            }
        }
    }
}
