using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomingPackagesQueueTests
{
    internal class when_PushSyncItem_called_and_NullReferenceException_were_thrown : IncomingPackagesQueueTestContext
    {
        Establish context = () =>
        {
            var fileSystemMock = new Mock<IFileSystemAccessor>();
            fileSystemMock.Setup(x => x.WriteAllText(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Throws<NullReferenceException>();
            incomingSyncPackagesQueue = CreateIncomingPackagesQueue(fileSystemAccessor: fileSystemMock.Object);
        };

        Because of = () => exception = Catch.Exception(() =>
            incomingSyncPackagesQueue.Enqueue(Guid.Empty, "nastya"));

        It should_throw_exception = () =>
          exception.ShouldNotBeNull();

        It should_throw_exception_of_type_NullReferenceException = () =>
          exception.ShouldBeOfExactType<NullReferenceException>();

        private static IncomingSyncPackagesQueue incomingSyncPackagesQueue;
        private static Exception exception;
    }
}
