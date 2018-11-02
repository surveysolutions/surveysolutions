using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.TabletInformation;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.FileBasedTabletInformationServiceTests
{
    [TestOf(typeof(FileBasedTabletInformationService))]
    internal class FileBasedTabletInformationServiceTests : FileBasedTabletInformationServiceTestContext
    {
        [Test]
        public void when_TabletInformation_is_saving()
        {
            // arrange
            byte[] savedContent = null;
            string savedFileName = null;
            string androidId = "aId";

            var content = new byte[] { 1, 2, 3 };

            var fileBasedTabletInformationService = CreateFileBasedTabletInformationService((fileName, contentToBeSave) =>
            {
                savedFileName = fileName;
                savedContent = contentToBeSave;
            });

            // act
            fileBasedTabletInformationService.SaveTabletInformation(content, androidId, null);

            // assert
            savedContent.Should().BeEquivalentTo(content);
            savedFileName.Should().EndWith(".zip");
            savedFileName.Should().Contain(androidId);

        }

        [Test]
        public void when_tabletInformation_is_saving_and_archive_has_encrypted_keystorage_keys_file()
        {
            // arrange
            var mockOfArchiveUtils = new Mock<IArchiveUtils>();
            mockOfArchiveUtils.Setup(x => x.GetArchivedFileNamesAndSize(It.IsAny<byte[]>()))
                .Returns(new Dictionary<string, long> {{"keystore.info", 1}});
            var mockOfEncryptionService = new Mock<IEncryptionService>();
            var mockOfFileSystemAccessor = new Mock<IFileSystemAccessor>();
            mockOfFileSystemAccessor.Setup(x => x.CombinePath(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>(Path.Combine);
            mockOfFileSystemAccessor.Setup(x => x.GetFileNameWithoutExtension(It.IsAny<string>())).Returns<string>(Path.GetFileNameWithoutExtension);

            var fileBasedTabletInformationService = CreateFileBasedTabletInformationService(
                archiveUtils: mockOfArchiveUtils.Object,
                encryptionService: mockOfEncryptionService.Object,
                fileSystemAccessor: mockOfFileSystemAccessor.Object);

            // act
            fileBasedTabletInformationService.SaveTabletInformation(new byte[] {1}, "android id", null);

            // assert
            mockOfFileSystemAccessor.Verify(x => x.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
            mockOfArchiveUtils.Verify(x => x.Unzip(It.IsAny<string>(), It.IsAny<string>(), true), Times.Once);
            mockOfEncryptionService.Verify(x => x.Decrypt(It.IsAny<string>()), Times.Once);
            mockOfFileSystemAccessor.Verify(x => x.WriteAllText(It.Is<string>(s => s.EndsWith("keystore.info")), It.IsAny<string>()), Times.Once);
            mockOfArchiveUtils.Verify(x => x.ZipDirectoryToFile(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockOfFileSystemAccessor.Verify(x => x.DeleteDirectory(It.IsAny<string>()), Times.Once);
        }

    }
}
