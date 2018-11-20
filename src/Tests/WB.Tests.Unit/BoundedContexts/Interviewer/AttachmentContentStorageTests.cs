using System.IO;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer
{
    public class AttachmentContentStorageTests
    {
        [Test]
        public void should_store_video_files_on_fileSystem()
        {
            var fs = new Mock<IFileSystemAccessor>();
            var contentId = Id.g10.FormatGuid();
            var content = new byte[] {1, 2, 3, 4};

            var storage = new AttachmentContentStorage(
                Mock.Of<IPlainStorage<AttachmentContentMetadata>>(),
                Mock.Of<IPlainStorage<AttachmentContentData>>(),
                fs.Object);

            var cachedFile = storage.GetFileCacheLocation(contentId);

            fs.Setup(f => f.IsFileExists(cachedFile)).Returns(false);
            fs.Setup(f => f.IsDirectoryExists(Path.GetDirectoryName(cachedFile))).Returns(true);
            
            storage.Store(new AttachmentContent
            {
                Id = contentId,
                ContentType = "video/create",
                Content = content,
                Size = content.Length
            });

            fs.Verify(f => f.WriteAllBytes(cachedFile, content), Times.Once);
        }


        [Test]
        public void should_not_store_video_files_on_fileSystem_if_exists()
        {
            var fs = new Mock<IFileSystemAccessor>();
            var contentId = Id.g10.FormatGuid();
            var content = new byte[] {1, 2, 3, 4};

            var storage = new AttachmentContentStorage(
                Mock.Of<IPlainStorage<AttachmentContentMetadata>>(),
                Mock.Of<IPlainStorage<AttachmentContentData>>(),
                fs.Object);

            var cachedFile = storage.GetFileCacheLocation(contentId);

            fs.Setup(f => f.IsFileExists(cachedFile)).Returns(true);
            fs.Setup(f => f.IsDirectoryExists(Path.GetDirectoryName(cachedFile))).Returns(true);
            
            storage.Store(new AttachmentContent
            {
                Id = contentId,
                ContentType = "video/destroy",
                Content = content,
                Size = content.Length
            });

            fs.Verify(f => f.WriteAllBytes(cachedFile, content), Times.Never);
        }
   
        [Test]
        public void should_create_directory_if_not_exists_while_storing_video()
        {
            var fs = new Mock<IFileSystemAccessor>();
            var contentId = Id.g10.FormatGuid();
            var content = new byte[] {1, 2, 3, 4};

            var storage = new AttachmentContentStorage(
                Mock.Of<IPlainStorage<AttachmentContentMetadata>>(),
                Mock.Of<IPlainStorage<AttachmentContentData>>(),
                fs.Object);

            var cachedFile = storage.GetFileCacheLocation(contentId);
            var cacheDir = Path.GetDirectoryName(cachedFile);
            fs.Setup(f => f.IsFileExists(cachedFile)).Returns(false);
            fs.Setup(f => f.IsDirectoryExists(cacheDir)).Returns(false);
            
            storage.Store(new AttachmentContent
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
        public void should_not_store_image_files()
        {
            var fs = new Mock<IFileSystemAccessor>();
            var contentId = Id.g10.FormatGuid();
            var content = new byte[] { 1, 2, 3, 4 };

            var storage = new AttachmentContentStorage(
                Mock.Of<IPlainStorage<AttachmentContentMetadata>>(),
                Mock.Of<IPlainStorage<AttachmentContentData>>(),
                fs.Object);

            var cachedFile = storage.GetFileCacheLocation(contentId);
            var cacheDir = Path.GetDirectoryName(cachedFile);
            fs.Setup(f => f.IsFileExists(cachedFile)).Returns(false);
            fs.Setup(f => f.IsDirectoryExists(cacheDir)).Returns(false);

            storage.Store(new AttachmentContent
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

    }
}