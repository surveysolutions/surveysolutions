using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Jobs;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Storage;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Tests.ExportJobTests
{
    [TestOf(typeof(ExportArchiveHandleService))]
    public class DeleteExportArchivesTests
    {
        private Mock<IFileBasedExportedDataAccessor> fileBasedExportedDataAccessor;
        private Mock<IExternalArtifactsStorage> externalArtifactsStorage;
        private Mock<IDataExportFileAccessor> exportFileAccessor;
        private Mock<ILogger<JobsStatusReporting>> logger;
        private ExportArchiveHandleService exportArchiveHandleService;
        private Mock<IFileSystemAccessor> fileSystemAccessor;
        
        private readonly string directory = "testDirectory";

        [SetUp]
        public void Setup()
        {
            this.fileBasedExportedDataAccessor = new Mock<IFileBasedExportedDataAccessor>();
            this.externalArtifactsStorage = new Mock<IExternalArtifactsStorage>();
            this.exportFileAccessor = new Mock<IDataExportFileAccessor>();
            this.logger = new Mock<ILogger<JobsStatusReporting>>();
            this.fileSystemAccessor = new Mock<IFileSystemAccessor>();

            this.exportArchiveHandleService = new ExportArchiveHandleService(
                fileBasedExportedDataAccessor.Object,
                externalArtifactsStorage.Object,
                exportFileAccessor.Object,
                logger.Object,
                Mock.Of<IExportFileNameService>(),
                fileSystemAccessor.Object);
            
            this.fileSystemAccessor.Object.CreateDirectory(directory);
            this.fileSystemAccessor.Setup(x=> x.IsDirectoryExists(directory)).Returns(true);
        }

        [Test]
        public async Task Should_Delete_All_Archives_When_No_Conditions_Are_Provided()
        {
            // Arrange
            var tenant = new TenantInfo("http://test", "testTenant");
            
            this.externalArtifactsStorage.Setup(x => x.IsEnabled()).Returns(false);
            this.fileBasedExportedDataAccessor.Setup(x => 
                x.GetExportDirectory(tenant)).Returns(directory);

            // Act
            await this.exportArchiveHandleService.ClearExportArchives(tenant);

            // Assert
            this.fileSystemAccessor.Verify(x => x.DeleteDirectory(directory), Times.Once);
        }

        [Test]
        public async Task Should_Delete_Archives_Based_On_DaysToKeep()
        {
            // Arrange
            var tenant = new TenantInfo("http://test", "testTenant");
           
            this.externalArtifactsStorage.Setup(x => x.IsEnabled()).Returns(false);
            this.fileBasedExportedDataAccessor.Setup(x => x.GetExportDirectory(tenant)).Returns(directory);

            var filesInDirectory = new Dictionary<string, DateTime>
            {
                { "old.zip", DateTime.UtcNow.AddDays(-10) },
                { "recent.zip", DateTime.UtcNow }
            };

            this.fileSystemAccessor.Setup(x => x.GetFilesInDirectory(directory, "*.zip", true))
                .Returns(filesInDirectory.Keys.ToArray());

            this.fileSystemAccessor.Setup(x => x.GetModificationTime(It.IsAny<string>()))
                .Returns((string filePath) => filesInDirectory[filePath]);
            
            // Act
            await this.exportArchiveHandleService.RunRetentionPolicy(tenant, null, 5);

            // Assert
            this.fileSystemAccessor.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Once);
            this.fileSystemAccessor.Verify(x => x.DeleteFile("old.zip"), Times.Once);
        }

        [Test]
        public async Task Should_Delete_Archives_Based_On_CountToKeep()
        {
            // Arrange
            var tenant = new TenantInfo("http://test", "testTenant");
            this.externalArtifactsStorage.Setup(x => x.IsEnabled()).Returns(false);
            this.fileBasedExportedDataAccessor.Setup(x => 
                x.GetExportDirectory(tenant)).Returns(directory);
            
            var filesInDirectory = new Dictionary<string, DateTime>
            {
                { "file1.zip", DateTime.UtcNow.AddDays(-3) },
                { "file2.zip", DateTime.UtcNow.AddDays(-2) },
                { "file3.zip", DateTime.UtcNow.AddDays(-1) }
            };
            this.fileSystemAccessor.Setup(x => x.GetFilesInDirectory(directory, "*.zip", true))
                .Returns(filesInDirectory.Keys.ToArray());
            
            this.fileSystemAccessor.Setup(x => x.GetModificationTime(It.IsAny<string>()))
                .Returns((string filePath) => filesInDirectory[filePath]);

            // Act
            await this.exportArchiveHandleService.RunRetentionPolicy(tenant, 2, null);

            // Assert
            this.fileSystemAccessor.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Once);
            this.fileSystemAccessor.Verify(x => x.DeleteFile("file1.zip"), Times.Once);
        }
        
        [Test]
        public async Task Should_Delete_Archives_Based_On_Both_CountToKeep_And_DaysToKeep()
        {
            // Arrange
            var tenant = new TenantInfo("http://test", "testTenant");
            this.externalArtifactsStorage.Setup(x => x.IsEnabled()).Returns(false);
            this.fileBasedExportedDataAccessor.Setup(x => x.GetExportDirectory(tenant)).Returns(directory);

            var filesInDirectory = new Dictionary<string, DateTime>
            {
                { "old.zip", DateTime.UtcNow.AddDays(-10) },
                { "recent1.zip", DateTime.UtcNow.AddDays(-3) },
                { "recent2.zip", DateTime.UtcNow.AddDays(-2) },
                { "recent3.zip", DateTime.UtcNow.AddDays(-1) }
            };
            this.fileSystemAccessor.Setup(x => x.GetFilesInDirectory(directory, "*.zip", true))
                .Returns(filesInDirectory.Keys.ToArray());
            
            this.fileSystemAccessor.Setup(x => x.GetModificationTime(It.IsAny<string>()))
                .Returns((string filePath) => filesInDirectory[filePath]);

            // Act
            await this.exportArchiveHandleService.RunRetentionPolicy(tenant, 2, 5);

            // Assert
            this.fileSystemAccessor.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Exactly(2));
            this.fileSystemAccessor.Verify(x => x.DeleteFile("old.zip"), Times.Once);
            this.fileSystemAccessor.Verify(x => x.DeleteFile("recent1.zip"), Times.Once);
        }
        
        [Test]
        public async Task Should_Not_Delete_Archives_When_Both_CountToKeep_And_DaysToKeep_Are_Not_Set()
        {
            // Arrange
            var tenant = new TenantInfo("http://test", "testTenant");
            this.externalArtifactsStorage.Setup(x => x.IsEnabled()).Returns(false);
            this.fileBasedExportedDataAccessor.Setup(x => x.GetExportDirectory(tenant)).Returns(directory);

            var filesInDirectory = new Dictionary<string, DateTime>
            {
                { "file1.zip", DateTime.UtcNow.AddDays(-3) },
                { "file2.zip", DateTime.UtcNow.AddDays(-2) }
            };
            this.fileSystemAccessor.Setup(x => x.GetFilesInDirectory(directory, "*.zip", true))
                .Returns(filesInDirectory.Keys.ToArray());
            
            this.fileSystemAccessor.Setup(x => x.GetModificationTime(It.IsAny<string>()))
                .Returns((string filePath) => filesInDirectory[filePath]);
            
            // Act
            await this.exportArchiveHandleService.RunRetentionPolicy(tenant, null, null);

            // Assert
            this.fileSystemAccessor.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never);
        }
        
        [Test]
        public async Task Should_Use_ExternalStorage_When_IsEnabled_Returns_True()
        {
            // Arrange
            var tenant = new TenantInfo("http://test", "testTenant");
            this.externalArtifactsStorage.Setup(x => x.IsEnabled()).Returns(true);
            this.externalArtifactsStorage.Setup(x => x.ListAsync(It.IsAny<string>()))
                .ReturnsAsync([new FileObject { Path = "file1.zip", LastModified = DateTime.UtcNow.AddDays(-10) }]);

            // Act
            await this.exportArchiveHandleService.RunRetentionPolicy(tenant, 1, null);

            // Assert
            this.externalArtifactsStorage.Verify(x => x.ListAsync(It.IsAny<string>()), Times.Once);
            this.externalArtifactsStorage.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Never); // No files to delete
        }
        
        [Test]
        public async Task Should_Use_ExternalStorage_When_IsEnabled_Returns_True_With_Both_CountToKeep_And_DaysToKeep()
        {
            // Arrange
            var tenant = new TenantInfo("http://test", "testTenant");
            this.externalArtifactsStorage.Setup(x => x.IsEnabled()).Returns(true);

            var files = new List<FileObject>
            {
                new FileObject { Path = "oldFile.zip", LastModified = DateTime.UtcNow.AddDays(-10) },
                new FileObject { Path = "recentFile1.zip", LastModified = DateTime.UtcNow.AddDays(-3) },
                new FileObject { Path = "recentFile2.zip", LastModified = DateTime.UtcNow.AddDays(-2) },
                new FileObject { Path = "recentFile3.zip", LastModified = DateTime.UtcNow.AddDays(-1) }
            };

            this.externalArtifactsStorage.Setup(x => x.ListAsync(It.IsAny<string>()))
                .ReturnsAsync(files);

            // Act
            await this.exportArchiveHandleService.RunRetentionPolicy(tenant, 2, 5);

            // Assert
            this.externalArtifactsStorage.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Exactly(2));
            this.externalArtifactsStorage.Verify(x => x.RemoveAsync("oldFile.zip"), Times.Once); // Deleted due to daysToKeep
            this.externalArtifactsStorage.Verify(x => x.RemoveAsync("recentFile1.zip"), Times.Once); // Deleted due to countToKeep
            this.externalArtifactsStorage.Verify(x => x.RemoveAsync("recentFile2.zip"), Times.Never); // Kept
            this.externalArtifactsStorage.Verify(x => x.RemoveAsync("recentFile3.zip"), Times.Never); // Kept
        }
    }
}
