using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Options;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.TabletInformation;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Infrastructure.Native.Workspaces;
using WB.Tests.Abc;

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
                fileSystemAccessorMock.Setup(x => x.GetFilesInDirectory(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(() => fileNamesInDirectory);

            var archiveUtils = Mock.Of<IArchiveUtils>(x =>
                x.GetArchivedFileNamesAndSize(It.IsAny<byte[]>()) == new Dictionary<string, long>());

            return new FileBasedTabletInformationService(Options.Create(new FileStorageConfig()), fileSystemAccessorMock.Object,
                archiveUtils, Create.Service.WorkspaceContextAccessor(), Mock.Of<IEncryptionService>());
        }

        protected static FileBasedTabletInformationService CreateFileBasedTabletInformationService(
            IFileSystemAccessor fileSystemAccessor = null,
            IArchiveUtils archiveUtils = null,
            IEncryptionService encryptionService = null) 
            => new FileBasedTabletInformationService(
                Options.Create(new FileStorageConfig()),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                archiveUtils ?? Mock.Of<IArchiveUtils>(),
                Create.Service.WorkspaceContextAccessor(),
                encryptionService ?? Mock.Of<IEncryptionService>());
    }
}
