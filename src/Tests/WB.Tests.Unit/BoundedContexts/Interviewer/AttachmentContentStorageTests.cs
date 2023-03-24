using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer
{
    public class AttachmentContentStorageTests
    {
        [Test]
        public async Task should_store_video_files_on_fileSystem()
        {
            var fs = new Mock<IFileSystemAccessor>();
            var contentId = Id.g10.FormatGuid();
            var content = new byte[] {1, 2, 3, 4};

            var storage = Create.Service.AttachmentContentStorage(files: fs.Object);

            var cachedFile = await storage.GetFileCacheLocationAsync(contentId);

            fs.Setup(f => f.IsFileExists(cachedFile)).Returns(false);
            fs.Setup(f => f.IsDirectoryExists(Path.GetDirectoryName(cachedFile))).Returns(true);
            
            await storage.StoreAsync(new AttachmentContent
            {
                Id = contentId,
                ContentType = "video/create",
                Content = content,
                Size = content.Length
            });

            fs.Verify(f => f.WriteAllBytes(cachedFile, content), Times.Once);
        }


        [Test]
        public async Task should_not_store_video_files_on_fileSystem_if_exists()
        {
            var fs = new Mock<IFileSystemAccessor>();
            var contentId = Id.g10.FormatGuid();
            var content = new byte[] {1, 2, 3, 4};

            var storage = Create.Service.AttachmentContentStorage(files: fs.Object);

            var cachedFile = await storage.GetFileCacheLocationAsync(contentId);

            fs.Setup(f => f.IsFileExists(cachedFile)).Returns(true);
            fs.Setup(f => f.IsDirectoryExists(Path.GetDirectoryName(cachedFile))).Returns(true);
            
            await storage.StoreAsync(new AttachmentContent
            {
                Id = contentId,
                ContentType = "video/destroy",
                Content = content,
                Size = content.Length
            });

            fs.Verify(f => f.WriteAllBytes(cachedFile, content), Times.Never);
        }
   
        [Test]
        public async Task should_create_directory_if_not_exists_while_storing_video()
        {
            var fs = new Mock<IFileSystemAccessor>();
            var contentId = Id.g10.FormatGuid();
            var content = new byte[] {1, 2, 3, 4};

            var storage = Create.Service.AttachmentContentStorage(files: fs.Object);

            var cachedFile = await storage.GetFileCacheLocationAsync(contentId);
            var cacheDir = Path.GetDirectoryName(cachedFile);
            fs.Setup(f => f.IsFileExists(cachedFile)).Returns(false);
            fs.Setup(f => f.IsDirectoryExists(cacheDir)).Returns(false);
            
            await storage.StoreAsync(new AttachmentContent
            {
                Id = contentId,
                ContentType = "video/content",
                Content = content,
                Size = content.Length
            });

            fs.Verify(f => f.CreateDirectory(cacheDir), Times.Once);
            fs.Verify(f => f.IsDirectoryExists(cacheDir), Times.Once);
        }

        [Test]
        public async Task should_not_store_image_files()
        {
            var fs = new Mock<IFileSystemAccessor>();
            var contentId = Id.g10.FormatGuid();
            var content = new byte[] { 1, 2, 3, 4 };

            var storage = Create.Service.AttachmentContentStorage(files: fs.Object);

            var cachedFile = await storage.GetFileCacheLocationAsync(contentId);
            var cacheDir = Path.GetDirectoryName(cachedFile);
            fs.Setup(f => f.IsFileExists(cachedFile)).Returns(false);
            fs.Setup(f => f.IsDirectoryExists(cacheDir)).Returns(false);

            await storage.StoreAsync(new AttachmentContent
            {
                Id = contentId,
                ContentType = "image/magic",
                Content = content,
                Size = content.Length
            });

            fs.Verify(f => f.CreateDirectory(cacheDir), Times.Never);
            fs.Verify(f => f.IsDirectoryExists(cacheDir), Times.Never);
            fs.Verify(f => f.IsFileExists(cachedFile), Times.Never);
        }


        [Test]
        public async Task should_get_video_files_on_fileSystem()
        {
            var fs = new Mock<IFileSystemAccessor>();
            var contentId = Id.g10.FormatGuid();
            var content = new byte[] { 1, 2, 3, 4 };

            var storage = Create.Service.AttachmentContentStorage(files: fs.Object);

            var cachedFile = await storage.GetFileCacheLocationAsync(contentId);

            fs.Setup(f => f.IsFileExists(cachedFile)).Returns(true);
            fs.Setup(f => f.IsDirectoryExists(Path.GetDirectoryName(cachedFile))).Returns(true);

            fs.Setup(f => f.ReadAllBytes(cachedFile, null, null)).Returns(content);
            
            await storage.GetContentAsync(contentId);

            var contentFromStorage = await storage.GetContentAsync(contentId);

            Assert.That(contentFromStorage, Is.EqualTo(content));
        }

        [Test]
        public async Task when_Exists_and_no_metadata_by_attachment_in_db_then_should_be_false()
        {
            // arrange
            var contentId = Id.g10.FormatGuid();
            var storage = Create.Service.AttachmentContentStorage();

            // act
            var isExists = await storage.ExistsAsync(contentId);

            // assert
            Assert.That(isExists, Is.False);
        }

        [Test]
        public async Task when_Exists_and_no_file_by_attachment_on_file_system_then_should_be_false()
        {
            // arrange
            var contentId = Id.g10.FormatGuid();
            var attachmentContentMetadataRepository = Create.Storage.SqliteInmemoryStorage(
                Create.Entity.AttachmentContentMetadata("image/png", contentId));
            var fileSystemAccessor = Mock.Of<IFileSystemAccessor>(x => x.IsFileExists(It.IsAny<string>()) == true);

            var storage = Create.Service.AttachmentContentStorage(
                attachmentContentMetadataRepository: attachmentContentMetadataRepository,
                files: fileSystemAccessor);

            // act
            var isExists = await storage.ExistsAsync(contentId);

            // assert
            Assert.That(isExists, Is.True);
        }

        [Test]
        public async Task when_Exists_and_no_attachment_content_in_db_and_on_file_system_then_should_be_false()
        {
            // arrange
            var contentId = Id.g10.FormatGuid();
            var attachmentContentMetadataRepository = Create.Storage.SqliteInmemoryStorage(
                Create.Entity.AttachmentContentMetadata("image/png", contentId));
            var fileSystemAccessor = Mock.Of<IFileSystemAccessor>(x => x.IsFileExists(It.IsAny<string>()) == false);

            var storage = Create.Service.AttachmentContentStorage(
                attachmentContentMetadataRepository: attachmentContentMetadataRepository,
                files: fileSystemAccessor);

            // act
            var isExists = await storage.ExistsAsync(contentId);

            // assert
            Assert.That(isExists, Is.False);
        }
    }
}
