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
    internal class when_DeQueue_called_and_folder_with_packages_is_empty : IncomePackagesRepositoryTestContext
    {
        Establish context = () =>
        {
            fileSystemMock = new Mock<IFileSystemAccessor>();

            incomingPackagesQueue = CreateIncomePackagesRepository(fileSystemAccessor: fileSystemMock.Object);
        };

        Because of = () => incomingPackagesQueue.DeQueue();

        It should_not_delete_any_files = () =>
          fileSystemMock.Verify(x=>x.DeleteFile(Moq.It.IsAny<string>()), Times.Never);

        private static IncomingPackagesQueue incomingPackagesQueue;

        private static Mock<IFileSystemAccessor> fileSystemMock;
    }
}
