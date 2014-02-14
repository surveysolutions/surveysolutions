using System;
using System.IO;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services.TabletInformation;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Supervisor.Tests.ServiceTests.FileBasedTabletInformationServiceTests
{
    internal class FileBasedTabletInformationServiceTestContext
    {
        protected static FileBasedTabletInformationService CreateFileBasedTabletInformationService(
            Action<string, byte[]> writeAllBytesCallback = null, string[] fileNamesInDirectory = null)
        {
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();

            fileSystemAccessorMock.Setup(x => x.CombinePath(It.IsAny<string[]>())).Returns<string[]>(Path.Combine);
            fileSystemAccessorMock.Setup(x => x.GetFileName(It.IsAny<string>())).Returns<string>((s) => s);
            fileSystemAccessorMock.Setup(x => x.GetCreationTime(It.IsAny<string>())).Returns(DateTime.Now);

            if (writeAllBytesCallback != null)
                fileSystemAccessorMock.Setup(x => x.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>())).Callback(
                    writeAllBytesCallback);

            if (fileNamesInDirectory != null)
                fileSystemAccessorMock.Setup(x => x.GetFilesInDirectory(It.IsAny<string>())).Returns(() => fileNamesInDirectory);

            return new FileBasedTabletInformationService(string.Empty, fileSystemAccessorMock.Object);
        }
    }
}
