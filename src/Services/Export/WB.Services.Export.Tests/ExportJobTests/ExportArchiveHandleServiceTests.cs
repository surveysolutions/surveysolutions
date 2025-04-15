using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
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
        
        private readonly string directory = "testDirectory";

        [SetUp]
        public void Setup()
        {
            this.fileBasedExportedDataAccessor = new Mock<IFileBasedExportedDataAccessor>();
            this.externalArtifactsStorage = new Mock<IExternalArtifactsStorage>();
            this.exportFileAccessor = new Mock<IDataExportFileAccessor>();
            this.logger = new Mock<ILogger<JobsStatusReporting>>();

            this.exportArchiveHandleService = new ExportArchiveHandleService(
                fileBasedExportedDataAccessor.Object,
                externalArtifactsStorage.Object,
                exportFileAccessor.Object,
                logger.Object,
                Mock.Of<IExportFileNameService>());
            
            Directory.CreateDirectory(directory);
        }
        
        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        [Test]
        public async Task Should_Delete_All_Archives_When_No_Conditions_Are_Provided()
        {
            // Arrange
            var tenant = new TenantInfo("http://test", "testTenant");
            
            this.externalArtifactsStorage.Setup(x => x.IsEnabled()).Returns(false);
            this.fileBasedExportedDataAccessor.Setup(x => 
                x.GetExportDirectory(tenant)).Returns(directory);
            
            var testFile = Path.Combine(directory, "test.zip");
            File.WriteAllText(testFile, "test content");

            // Act
            await this.exportArchiveHandleService.ClearExportArchives(tenant);

            // Assert
            Assert.That(Directory.Exists(directory), Is.False);
        }

        [Test]
        public async Task Should_Delete_Archives_Based_On_DaysToKeep()
        {
            // Arrange
            var tenant = new TenantInfo("http://test", "testTenant");
           
            this.externalArtifactsStorage.Setup(x => x.IsEnabled()).Returns(false);
            this.fileBasedExportedDataAccessor.Setup(x => x.GetExportDirectory(tenant)).Returns(directory);

            var oldFile = Path.Combine(directory, "old.zip");
            var recentFile = Path.Combine(directory, "recent.zip");
            File.WriteAllText(oldFile, "old content");
            File.WriteAllText(recentFile, "recent content");
            File.SetLastWriteTimeUtc(oldFile, DateTime.UtcNow.AddDays(-10));
            File.SetLastWriteTimeUtc(recentFile, DateTime.UtcNow);

            // Act
            await this.exportArchiveHandleService.RunRetentionPolicy(tenant, null, 5);

            // Assert
            Assert.That(File.Exists(oldFile), Is.False);
            Assert.That(File.Exists(recentFile), Is.True);
        }

        [Test]
        public async Task Should_Delete_Archives_Based_On_CountToKeep()
        {
            // Arrange
            var tenant = new TenantInfo("http://test", "testTenant");
            this.externalArtifactsStorage.Setup(x => x.IsEnabled()).Returns(false);
            this.fileBasedExportedDataAccessor.Setup(x => 
                x.GetExportDirectory(tenant)).Returns(directory);

            var file1 = Path.Combine(directory, "file1.zip");
            var file2 = Path.Combine(directory, "file2.zip");
            var file3 = Path.Combine(directory, "file3.zip");
            File.WriteAllText(file1, "content1");
            File.WriteAllText(file2, "content2");
            File.WriteAllText(file3, "content3");
            File.SetLastWriteTimeUtc(file1, DateTime.UtcNow.AddDays(-3));
            File.SetLastWriteTimeUtc(file2, DateTime.UtcNow.AddDays(-2));
            File.SetLastWriteTimeUtc(file3, DateTime.UtcNow.AddDays(-1));

            // Act
            await this.exportArchiveHandleService.RunRetentionPolicy(tenant, 2, null);

            // Assert
            Assert.That(File.Exists(file1), Is.False);
            Assert.That(File.Exists(file2), Is.True);
            Assert.That(File.Exists(file3), Is.True);
        }
        
        [Test]
        public async Task Should_Delete_Archives_Based_On_Both_CountToKeep_And_DaysToKeep()
        {
            // Arrange
            var tenant = new TenantInfo("http://test", "testTenant");
            this.externalArtifactsStorage.Setup(x => x.IsEnabled()).Returns(false);
            this.fileBasedExportedDataAccessor.Setup(x => x.GetExportDirectory(tenant)).Returns(directory);

            var oldFile = Path.Combine(directory, "old.zip");
            var recentFile1 = Path.Combine(directory, "recent1.zip");
            var recentFile2 = Path.Combine(directory, "recent2.zip");
            var recentFile3 = Path.Combine(directory, "recent3.zip");

            File.WriteAllText(oldFile, "old content");
            File.WriteAllText(recentFile1, "recent content 1");
            File.WriteAllText(recentFile2, "recent content 2");
            File.WriteAllText(recentFile3, "recent content 3");

            File.SetLastWriteTimeUtc(oldFile, DateTime.UtcNow.AddDays(-10));
            File.SetLastWriteTimeUtc(recentFile1, DateTime.UtcNow.AddDays(-3));
            File.SetLastWriteTimeUtc(recentFile2, DateTime.UtcNow.AddDays(-2));
            File.SetLastWriteTimeUtc(recentFile3, DateTime.UtcNow.AddDays(-1));

            // Act
            await this.exportArchiveHandleService.RunRetentionPolicy(tenant, 2, 5);

            // Assert
            Assert.That(File.Exists(oldFile), Is.False); // Deleted due to daysToKeep
            Assert.That(File.Exists(recentFile1), Is.False); // Deleted due to countToKeep
            Assert.That(File.Exists(recentFile2), Is.True); // Kept
            Assert.That(File.Exists(recentFile3), Is.True); // Kept
        }
        
        [Test]
        public async Task Should_Not_Delete_Archives_When_Both_CountToKeep_And_DaysToKeep_Are_Not_Set()
        {
            // Arrange
            var tenant = new TenantInfo("http://test", "testTenant");
            this.externalArtifactsStorage.Setup(x => x.IsEnabled()).Returns(false);
            this.fileBasedExportedDataAccessor.Setup(x => x.GetExportDirectory(tenant)).Returns(directory);

            var file1 = Path.Combine(directory, "file1.zip");
            var file2 = Path.Combine(directory, "file2.zip");

            File.WriteAllText(file1, "content1");
            File.WriteAllText(file2, "content2");

            File.SetLastWriteTimeUtc(file1, DateTime.UtcNow.AddDays(-3));
            File.SetLastWriteTimeUtc(file2, DateTime.UtcNow.AddDays(-1));

            // Act
            await this.exportArchiveHandleService.RunRetentionPolicy(tenant, null, null);

            // Assert
            Assert.That(File.Exists(file1), Is.True); // File should not be deleted
            Assert.That(File.Exists(file2), Is.True); // File should not be deleted
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
            this.externalArtifactsStorage.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once); // No files to delete
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
            this.externalArtifactsStorage.Verify(x => x.RemoveAsync("oldFile.zip"), Times.Once); // Deleted due to daysToKeep
            this.externalArtifactsStorage.Verify(x => x.RemoveAsync("recentFile1.zip"), Times.Once); // Deleted due to countToKeep
            this.externalArtifactsStorage.Verify(x => x.RemoveAsync("recentFile2.zip"), Times.Never); // Kept
            this.externalArtifactsStorage.Verify(x => x.RemoveAsync("recentFile3.zip"), Times.Never); // Kept
        }
    }
}
