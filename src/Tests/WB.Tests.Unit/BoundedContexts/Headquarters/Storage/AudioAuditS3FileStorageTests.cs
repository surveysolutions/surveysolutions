using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Storage
{
    [TestFixture]
    [TestOf(typeof(AudioAuditS3FileStorage))]
    public class AudioAuditS3FileStorageTests
    {
        private Mock<IExternalFileStorage> externalFileStorage;
        private Mock<IPlainStorageAccessor<AudioAuditFile>> plainStorage;
        private AudioAuditS3FileStorage storage;

        [SetUp]
        public void SetUp()
        {
            this.externalFileStorage = new Mock<IExternalFileStorage>();
            this.plainStorage = new Mock<IPlainStorageAccessor<AudioAuditFile>>();
            this.storage = new AudioAuditS3FileStorage(this.externalFileStorage.Object, this.plainStorage.Object,
                Mock.Of<IUnitOfWork>());
        }

        [Test]
        public async Task when_removing_all_binary_data_for_interviews_should_remove_stored_objects_and_metadata()
        {
            var interviewId = Guid.NewGuid();
            var prefix = $"audio_audit/{interviewId}#";

            this.externalFileStorage.SetupSequence(s => s.ListAsync(prefix))
                .ReturnsAsync(new List<FileObject> { new FileObject { Path = prefix + "audio.mp3" } })
                .ReturnsAsync(new List<FileObject>());

            Func<IQueryable<AudioAuditFile>, IQueryable<AudioAuditFile>> removeQuery = null;
            this.plainStorage.Setup(s => s.Remove(It.IsAny<Func<IQueryable<AudioAuditFile>, IQueryable<AudioAuditFile>>>()))
                .Callback<Func<IQueryable<AudioAuditFile>, IQueryable<AudioAuditFile>>>(query => removeQuery = query);

            await this.storage.RemoveAllBinaryDataForInterviewsAsync(new List<Guid> { interviewId });

            this.externalFileStorage.Verify(s => s.RemoveAsync(
                    It.Is<IEnumerable<string>>(paths => paths.SequenceEqual(new[] { prefix + "audio.mp3" }))),
                Times.Once);

            removeQuery.Should().NotBeNull();

            var files = new List<AudioAuditFile>
            {
                new AudioAuditFile { Id = "1", InterviewId = interviewId },
                new AudioAuditFile { Id = "2", InterviewId = Guid.NewGuid() }
            };

            removeQuery(files.AsQueryable()).Select(f => f.Id).Should().BeEquivalentTo(new[] { "1" });
        }
    }
}
