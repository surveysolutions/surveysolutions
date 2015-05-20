using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.Synchronization;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomingPackagesQueueTests
{
    [Subject(typeof(IncomingSyncPackagesQueue))]
    internal class IncomingPackagesQueueTestContext
    {
        protected static IncomingSyncPackagesQueue CreateIncomingPackagesQueue(IJsonUtils jsonUtils = null,
            IFileSystemAccessor fileSystemAccessor = null, IArchiveUtils archiver = null)
        {
            return new IncomingSyncPackagesQueue(fileSystemAccessor??Mock.Of<IFileSystemAccessor>(),
                new SyncSettings(AppDataDirectory, IncomingCapiPackagesWithErrorsDirectoryName,
                    IncomingCapiPackageFileNameExtension, IncomingCapiPackagesDirectoryName, "",3,1), Mock.Of<ILogger>(), jsonUtils: jsonUtils ?? Mock.Of<IJsonUtils>(),
                archiver: archiver ?? Mock.Of<IArchiveUtils>());

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
