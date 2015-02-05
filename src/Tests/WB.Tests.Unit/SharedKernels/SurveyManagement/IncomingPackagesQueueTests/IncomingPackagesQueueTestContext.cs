using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository;
using WB.Core.Synchronization;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomingPackagesQueueTests
{
    [Subject(typeof(IncomingPackagesQueue))]
    internal class IncomingPackagesQueueTestContext
    {
        protected static IncomingPackagesQueue CreateIncomingPackagesQueue(IFileSystemAccessor fileSystemAccessor = null)
        {
            return new IncomingPackagesQueue(fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(), new SyncSettings(AppDataDirectory, IncomingCapiPackagesWithErrorsDirectoryName, IncomingCapiPackageFileNameExtension, IncomingCapiPackagesDirectoryName, ""));
        }

        protected static Mock<IFileSystemAccessor> CreateDefaultFileSystemAccessorMock()
        {
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);
            return fileSystemAccessorMock;
        }

        const string AppDataDirectory = "App_Data";
        const string IncomingCapiPackagesDirectoryName = "IncomingData";
        const string IncomingCapiPackagesWithErrorsDirectoryName = "IncomingDataWithErrors";
        const string IncomingCapiPackageFileNameExtension = "sync";
    }
}
