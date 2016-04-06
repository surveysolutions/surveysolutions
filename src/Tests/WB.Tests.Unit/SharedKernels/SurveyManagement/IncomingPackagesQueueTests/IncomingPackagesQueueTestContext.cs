using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.Synchronization;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomingPackagesQueueTests
{
    [Subject(typeof(IncomingSyncPackagesQueue))]
    internal class IncomingPackagesQueueTestContext
    {
        protected static IncomingSyncPackagesQueue CreateIncomingPackagesQueue(ISerializer serializer = null,
            IFileSystemAccessor fileSystemAccessor = null, IArchiveUtils archiver = null, 
            IPlainStorageAccessor<InterviewPackage> interviewPackageStorage = null,
            IPlainStorageAccessor<BrokenInterviewPackage> brokenInterviewPackageStorage = null,
            ICommandService commandService = null)
        {
            return new IncomingSyncPackagesQueue(
                fileSystemAccessor??Mock.Of<IFileSystemAccessor>(),
                new SyncSettings(AppDataDirectory, IncomingCapiPackagesWithErrorsDirectoryName, IncomingCapiPackageFileNameExtension, IncomingCapiPackagesDirectoryName, "",3,1), 
                Mock.Of<ILogger>(), 
                serializer: serializer ?? Mock.Of<ISerializer>(),
                archiver: archiver ?? Mock.Of<IArchiveUtils>(), 
                interviewPackageStorage: interviewPackageStorage ?? Mock.Of<IPlainStorageAccessor<InterviewPackage>>(),
                brokenInterviewPackageStorage: brokenInterviewPackageStorage ?? Mock.Of<IPlainStorageAccessor<BrokenInterviewPackage>>(),
                commandService: commandService ?? Mock.Of<ICommandService>());
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
