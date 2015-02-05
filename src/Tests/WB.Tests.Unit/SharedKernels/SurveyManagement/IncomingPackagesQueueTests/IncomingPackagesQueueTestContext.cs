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
using WB.Core.Synchronization;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomingPackagesQueueTests
{
    [Subject(typeof(IncomingPackagesQueue))]
    internal class IncomingPackagesQueueTestContext
    {
        protected static IncomingPackagesQueue CreateIncomingPackagesQueue(IFileSystemAccessor fileSystemAccessor = null)
        {
            return new IncomingPackagesQueue(fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(), new SyncSettings(appDataDirectory, incomingCapiPackagesWithErrorsDirectoryName, incomingCapiPackageFileNameExtension, incomingCapiPackagesDirectoryName, ""));
        }

        protected static Mock<IFileSystemAccessor> CreateDefaultFileSystemAccessorMock()
        {
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);
            return fileSystemAccessorMock;
        }

        const string appDataDirectory = "App_Data";
        const string incomingCapiPackagesDirectoryName = "IncomingData";
        const string incomingCapiPackagesWithErrorsDirectoryName = "IncomingDataWithErrors";
        const string incomingCapiPackageFileNameExtension = "sync";
    }
}
