using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
