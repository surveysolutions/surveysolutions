using System;
using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.Synchronization;
using WB.Tests.Unit.SharedKernels.SurveyManagement.SyncPackagesProcessorTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomingPackagesQueueTests
{
    internal class when_DeQueue_called_and_package_cant_be_opened : IncomingPackagesQueueTestContext
    {
        Establish context = () =>
        {
            var jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserialize<SyncItem>(Moq.It.IsAny<string>())).Throws<NullReferenceException>();

            fileSystemAccessorMock=new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.IsFileExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessorMock.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>()))
                .Returns(new[] {interviewId.FormatGuid()});
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);

            incomingSyncPackagesQueue = CreateIncomingPackagesQueue(fileSystemAccessor: fileSystemAccessorMock.Object,
                jsonUtils: jsonUtilsMock.Object);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
               result = incomingSyncPackagesQueue.DeQueue()) as IncomingSyncPackageException;

        It should_throw_exception_with_interviewId_equals_to_null = () =>
            exception.InterviewId.ShouldBeNull();

        It should_result_be_null = () =>
           result.ShouldBeNull();

        private static IncomingSyncPackagesQueue incomingSyncPackagesQueue;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static Guid interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static IncomingSyncPackage result;
        private static IncomingSyncPackageException exception;
    }
}
