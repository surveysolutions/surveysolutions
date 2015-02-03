using System;
using System.Collections.Generic;
using System.IO;
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
    internal class when_GetUnhandledPackagePath_is_called : IncomePackagesRepositoryTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);

            incomingPackagesQueue = CreateIncomePackagesRepository(fileSystemAccessor: fileSystemAccessorMock.Object);
        };

        Because of = () =>
            result = incomingPackagesQueue.GetUnhandledPackagePath("test");

        It should_result_be_empty = () =>
           result.ShouldEqual(@"App_Data\IncomingDataWithErrors\test");

        private static IncomingPackagesQueue incomingPackagesQueue;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static string result;
    }
}
