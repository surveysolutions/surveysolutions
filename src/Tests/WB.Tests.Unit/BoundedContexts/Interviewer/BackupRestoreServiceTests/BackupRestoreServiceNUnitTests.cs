using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.BackupRestoreServiceTests
{
    [TestFixture]
    internal class BackupRestoreServiceNUnitTests
    {
        [Test]
        public async void GetSystemBackupAsync_When_crush_file_exists_in_external_storage_Then_crush_file_should_be_included_in_backup()
        {
            var archiveUtilsMock=new Mock<IArchiveUtils>();
            var asynchronousFileSystemAccessorMock = new Mock<IAsynchronousFileSystemAccessor>();

            asynchronousFileSystemAccessorMock.Setup(x => x.IsFileExistsAsync(Moq.It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            var backupRestoreService = CreateBackupRestoreService(archiveUtilsMock.Object,
                asynchronousFileSystemAccessorMock.Object);

            await backupRestoreService.GetSystemBackupAsync();

            asynchronousFileSystemAccessorMock.Verify(x => x.CopyFileAsync("crush", "private"), Times.Once);

            archiveUtilsMock.Verify(x => x.ZipDirectoryToByteArrayAsync("private", null,  @"\.log$;\.dll$;\.mdb$;"), Times.Once);
        }

        [Test]
        public async void RestoreAsync_When_restore_file_present_Then_private_storage_data_should_be_replaced_with_unzipped_backup_content()
        {
            var archiveUtilsMock = new Mock<IArchiveUtils>();
            var asynchronousFileSystemAccessorMock = new Mock<IAsynchronousFileSystemAccessor>();

            asynchronousFileSystemAccessorMock.Setup(x => x.IsFileExistsAsync(Moq.It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            var backupRestoreService = CreateBackupRestoreService(archiveUtilsMock.Object,
                asynchronousFileSystemAccessorMock.Object);

            await backupRestoreService.RestoreAsync("backup");

            archiveUtilsMock.Verify(x => x.UnzipAsync("backup", "private", true),
                Times.Once);
        }

        [Test]
        public async void BackupAsync_When_backup_folder_is_absent_Then_backup_folder_should_be_created_and_backup_stored_in_it()
        {
            var archiveUtilsMock = new Mock<IArchiveUtils>();
            var asynchronousFileSystemAccessorMock = new Mock<IAsynchronousFileSystemAccessor>();

            asynchronousFileSystemAccessorMock.Setup(x => x.IsDirectoryExistsAsync(Moq.It.IsAny<string>()))
                .Returns(Task.FromResult(false));
            asynchronousFileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);

            var backupRestoreService = CreateBackupRestoreService(archiveUtilsMock.Object,
                asynchronousFileSystemAccessorMock.Object);

            await backupRestoreService.BackupAsync("backup");

            asynchronousFileSystemAccessorMock.Verify(x => x.CreateDirectoryAsync("backup"), Times.Once);
            asynchronousFileSystemAccessorMock.Verify(
                x =>
                    x.WriteAllBytesAsync(
                        Moq.It.Is<string>(_ => _.Contains(@"backup\backup-interviewer-") && _.Contains(@".ibak")),
                        Moq.It.IsAny<byte[]>()), Times.Once);
        }

        private BackupRestoreService CreateBackupRestoreService(
            IArchiveUtils archiver=null,
            IAsynchronousFileSystemAccessor fileSystemAccessor = null)
        {
            return new BackupRestoreService(archiver ?? Mock.Of<IArchiveUtils>(),
                fileSystemAccessor ?? Mock.Of<IAsynchronousFileSystemAccessor>(), Mock.Of<IInterviewerSettings>(_=>_.CrushFilePath=="crush"), "private");
        }
    }
}