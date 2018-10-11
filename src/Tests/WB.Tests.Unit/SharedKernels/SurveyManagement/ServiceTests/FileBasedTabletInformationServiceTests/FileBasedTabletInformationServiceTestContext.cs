using System;
using System.IO;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.TabletInformation;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.FileBasedTabletInformationServiceTests
{
    internal class FileBasedTabletInformationServiceTestContext
    {
        protected static FileBasedTabletInformationService CreateFileBasedTabletInformationService(
            Action<string, byte[]> writeAllBytesCallback = null, string[] fileNamesInDirectory = null)
        {
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();

            fileSystemAccessorMock.Setup(x => x.CombinePath(It.IsAny<string>(), It.IsAny<string>())).Returns<string,string>(Path.Combine);
            fileSystemAccessorMock.Setup(x => x.GetFileName(It.IsAny<string>())).Returns<string>((s) => s);
            fileSystemAccessorMock.Setup(x => x.GetCreationTime(It.IsAny<string>())).Returns(DateTime.Now);
            fileSystemAccessorMock.Setup(x => x.GetFileNameWithoutExtension(It.IsAny<string>())).Returns<string>(Path.GetFileNameWithoutExtension);

            if (writeAllBytesCallback != null)
                fileSystemAccessorMock.Setup(x => x.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>())).Callback(
                    writeAllBytesCallback);

            if (fileNamesInDirectory != null)
                fileSystemAccessorMock.Setup(x => x.GetFilesInDirectory(It.IsAny<string>(), It.IsAny<string>())).Returns(() => fileNamesInDirectory);

            return new FileBasedTabletInformationService(string.Empty, fileSystemAccessorMock.Object,
                Mock.Of<IArchiveUtils>(), Mock.Of<IEncryptionService>());
        }
    }
}
