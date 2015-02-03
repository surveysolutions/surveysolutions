using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomePackagesRepositoryTests
{
    internal class when_PushSyncItem_called_and_NullReferenceException_were_thrown : IncomePackagesRepositoryTestContext
    {
        Establish context = () =>
        {
            var fileSystemMock = new Mock<IFileSystemAccessor>();
            fileSystemMock.Setup(x => x.WriteAllText(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Throws<NullReferenceException>();
            incomingPackagesQueue = CreateIncomePackagesRepository(fileSystemAccessor: fileSystemMock.Object);
        };

        Because of = () => exception = Catch.Exception(() =>
            incomingPackagesQueue.PushSyncItem("nastya"));

        It should_throw_exception = () =>
          exception.ShouldNotBeNull();

        It should_throw_exception_of_type_NullReferenceException = () =>
          exception.ShouldBeOfExactType<NullReferenceException>();

        private static IncomingPackagesQueue incomingPackagesQueue;
        private static Exception exception;
    }
}
