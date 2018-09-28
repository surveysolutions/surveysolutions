using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Services
{
    [TestOf(typeof(InterviewerUpdateHandler))]
    public class InterviewerUpdateHandlerTests
    {
        [Test]
        public async Task should_return_null_content_if_no_files_present()
        {
            var fileAccess = Mock.Of<IFileSystemAccessor>(f => f.IsFileExists(It.IsAny<string>()) == false);
            var settings = Mock.Of<ISupervisorSettings>(ss => ss.DownloadUpdatesForInterviewerApp == true);

            var handler = Create.Service.InterviewerUpdateHandler(fileAccess, settings);

            var result = await handler.GetInterviewerApp(new GetInterviewerAppRequest(1, EnumeratorApplicationType.WithoutMaps));

            Assert.That(result.Content, Is.Null);
            Assert.That(result.Length, Is.EqualTo(0));
        }

        [Test]
        public async Task should_return_only_requested_chunk_bytes()
        {
            var fileSize = 3;
            var chunk = new byte[] {2};

            var fileAccess = new Mock<IFileSystemAccessor>();
            fileAccess
                .Setup(f => f.IsFileExists(It.IsAny<string>()))
                .Returns(true);
            fileAccess
                .Setup(f => f.ReadAllBytes(It.IsAny<string>(), 1, 1))
                .Returns(chunk);
            fileAccess
                .Setup(f => f.GetFileSize(It.IsAny<string>()))
                .Returns(fileSize);

            var settings = Mock.Of<ISupervisorSettings>(ss => ss.DownloadUpdatesForInterviewerApp == true);

            var handler = Create.Service.InterviewerUpdateHandler(fileAccess.Object, settings);

            var result = await handler.GetInterviewerApp(new GetInterviewerAppRequest(1, EnumeratorApplicationType.WithoutMaps)
            {
                Skip = 1,
                Maximum = 1
            });

            Assert.That(result.Content, Is.EqualTo(chunk));
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result.Total, Is.EqualTo(fileSize));
        }
    }
}
