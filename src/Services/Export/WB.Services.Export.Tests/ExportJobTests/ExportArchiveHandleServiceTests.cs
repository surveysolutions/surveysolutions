using System;
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
    public class DeleteExportArchivesTests
    {
        private Mock<IFileBasedExportedDataAccessor> fileBasedExportedDataAccessor;
        private Mock<IExternalArtifactsStorage> externalArtifactsStorage;
        private Mock<IDataExportFileAccessor> exportFileAccessor;
        private Mock<ILogger<JobsStatusReporting>> logger;
        private ExportArchiveHandleService exportArchiveHandleService;
        
        private string directory = "testDirectory";

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
    }
}
