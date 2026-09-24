using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Storage;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Storage
{
    [TestFixture]
    [TestOf(typeof(ExternalFileStorageExtensions))]
    public class ExternalFileStorageExtensionsTests
    {
        private const int MaxDeletionBatches = 1000;

        [Test]
        [CancelAfter(2000)]
        public void when_removing_prefix_and_objects_remain_after_all_batches_should_throw()
        {
            var prefix = "images/interview/";
            var externalFileStorage = new Mock<IExternalFileStorage>();

            externalFileStorage.Setup(s => s.ListAsync(prefix))
                .ReturnsAsync(new List<FileObject> { new FileObject { Path = prefix + "1.jpg" } });

            var exception = Assert.ThrowsAsync<InvalidOperationException>(() =>
                externalFileStorage.Object.RemoveAllUnderPrefixAsync(prefix));

            Assert.That(exception!.Message, Is.EqualTo($"Unable to remove all files stored under '{prefix}'."));
            externalFileStorage.Verify(s => s.RemoveAsync(
                It.Is<IEnumerable<string>>(paths => paths.SequenceEqual(new[] { prefix + "1.jpg" }))),
                Times.Exactly(MaxDeletionBatches));
            externalFileStorage.Verify(s => s.ListAsync(prefix), Times.Exactly(MaxDeletionBatches + 1));
        }

        [Test]
        public async Task when_removing_prefix_and_last_allowed_batch_clears_objects_should_complete()
        {
            var prefix = "images/interview/";
            var listCallsCount = 0;
            var externalFileStorage = new Mock<IExternalFileStorage>();

            externalFileStorage.Setup(s => s.ListAsync(prefix))
                .ReturnsAsync(() => ++listCallsCount > MaxDeletionBatches
                    ? new List<FileObject>()
                    : new List<FileObject> { new FileObject { Path = prefix + listCallsCount } });

            await externalFileStorage.Object.RemoveAllUnderPrefixAsync(prefix);

            externalFileStorage.Verify(s => s.RemoveAsync(It.IsAny<IEnumerable<string>>()),
                Times.Exactly(MaxDeletionBatches));
            externalFileStorage.Verify(s => s.ListAsync(prefix), Times.Exactly(MaxDeletionBatches + 1));
        }
    }
}
