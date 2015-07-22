using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using WB.Core.GenericSubdomains.ErrorReporting;
using WB.Core.GenericSubdomains.ErrorReporting.Implementation.CapiInformation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Tests.Unit.GenericSubdomains.ErrorReporting.CapiInformationServiceTests
{
    internal class CapiInformationServiceTestContext
    {
        protected static CapiInformationService CreateCapiInformationService(string[] filesToArchive, Action<string> addToArchiveCallback,
            IArchiveUtils archiveUtils)
        {
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.CombinePath(It.IsAny<string>(), It.IsAny<string>())).Returns<string,string>(Path.Combine);

            fileSystemAccessorMock.Setup(x => x.CopyFileOrDirectory(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((sourceDir, targetDir) => addToArchiveCallback(sourceDir));

            fileSystemAccessorMock.Setup(x => x.IsFileExists(It.IsAny<string>())).Returns(true);

            return new CapiInformationService(
                Mock.Of<IInfoFileSupplierRegistry>(_ => _.GetAll() == filesToArchive),
                fileSystemAccessorMock.Object,
                archiveUtils,
                string.Empty);
        }
    }
}
