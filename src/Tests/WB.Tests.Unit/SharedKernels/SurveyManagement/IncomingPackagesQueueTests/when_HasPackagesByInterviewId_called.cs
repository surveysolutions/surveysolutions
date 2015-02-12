using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomingPackagesQueueTests
{
    internal class when_HasPackagesByInterviewId_called : IncomingPackagesQueueTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns(filesInFolder);

            incomingSyncPackagesQueue = CreateIncomingPackagesQueue(fileSystemAccessor: fileSystemAccessorMock.Object);
        };

        Because of = () =>
            result = incomingSyncPackagesQueue.HasPackagesByInterviewId(new Guid());

        It should_sync_packages_folder_contains_some_sync_files_by_specific_interview_id = () =>
           result.ShouldBeTrue();

        private static IncomingSyncPackagesQueue incomingSyncPackagesQueue;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
       
        private static bool result;
        private static string[] filesInFolder = new[] { "datetimeticks-interviewid.sync", "datetimeticks-interviewid.sync" };
    }
}
