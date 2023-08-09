using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Api;
using Xamarin.Essentials;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public class AttachmentContentStorage : IAttachmentContentStorage
    {
        private readonly IPlainStorage<AttachmentContentMetadata> attachmentContentMetadataRepository;
        private readonly IPlainStorage<AttachmentContentData> attachmentContentDataRepository;
        private readonly IPlainStorage<AttachmentPreviewContentData> attachmentPreviewContentDataRepository;
        private readonly IPathUtils pathUtils;
        private readonly IPermissionsService permissionsService;
        private readonly IFileSystemAccessor files;
        private readonly IImageHelper imageHelper;

        public AttachmentContentStorage(
            IPlainStorage<AttachmentContentMetadata> attachmentContentMetadataRepository,
            IPlainStorage<AttachmentContentData> attachmentContentDataRepository,
            IPlainStorage<AttachmentPreviewContentData> attachmentPreviewContentDataRepository,
            IPathUtils pathUtils,
            IPermissionsService permissionsService,
            IFileSystemAccessor files,
            IImageHelper imageHelper)
        {
            this.attachmentContentMetadataRepository = attachmentContentMetadataRepository;
            this.attachmentContentDataRepository = attachmentContentDataRepository;
            this.pathUtils = pathUtils;
            this.permissionsService = permissionsService;
            this.files = files;
            this.attachmentPreviewContentDataRepository = attachmentPreviewContentDataRepository;
            this.imageHelper = imageHelper;
        }

        public async Task StoreAsync(AttachmentContent attachmentContent)
        {
            var storeInFileSystem = IsStoredInFileSystem(attachmentContent);

            this.attachmentContentMetadataRepository.Store(new AttachmentContentMetadata
            {
                ContentType = attachmentContent.ContentType,
                Id = attachmentContent.Id,
                Size = attachmentContent.Size,
            });

            if (!storeInFileSystem)
            {
                this.attachmentContentDataRepository.Store(new AttachmentContentData
                {
                    Id = attachmentContent.Id,
                    Content = attachmentContent.Content
                });

                byte[] previewContent = imageHelper.GetTransformedArrayOrNull(attachmentContent.Content, 400);
                if (previewContent != null)
                {
                    this.attachmentPreviewContentDataRepository.Store(new AttachmentPreviewContentData
                    {
                        Id = attachmentContent.Id,
                        PreviewContent = previewContent
                    });
                }
            }
            else
            {
                var fileCache = await GetFileCacheLocationAsync(attachmentContent.Id);

                if (!files.IsFileExists(fileCache))
                {
                    var cacheDirectory = await GetFileCacheDirectoryAsync();
                    if (!files.IsDirectoryExists(cacheDirectory))
                    {
                        files.CreateDirectory(cacheDirectory);
                    }

                    files.WriteAllBytes(fileCache, attachmentContent.Content);
                }
            }
        }

        private bool IsStoredInFileSystem(AttachmentContent attachmentContent)
        {
            return attachmentContent.IsVideo() || attachmentContent.IsPdf() || attachmentContent.IsAudio();
        }

        public async Task RemoveAsync(string attachmentContentId)
        {
            this.attachmentContentMetadataRepository.Remove(attachmentContentId);
            this.attachmentContentDataRepository.Remove(attachmentContentId);
            this.attachmentPreviewContentDataRepository.Remove(attachmentContentId);

            var fileCache = await GetFileCacheLocationAsync(attachmentContentId);
            if (files.IsFileExists(fileCache))
            {
                files.DeleteFile(fileCache);
            }
        }

        public AttachmentContentMetadata GetMetadata(string attachmentContentId)
        {
            var attachmentContent = this.attachmentContentMetadataRepository.GetById(attachmentContentId);
            return attachmentContent;
        }

        public async Task<bool> ExistsAsync(string attachmentContentId)
        {
            var attachmentContentMeta = this.GetMetadata(attachmentContentId);
            if (attachmentContentMeta == null) return false;

            var fileCache = await GetFileCacheLocationAsync(attachmentContentId);
            if (files.IsFileExists(fileCache)) return true;

            return this.attachmentContentDataRepository.Count(x =>
                x.Id == attachmentContentId && x.Content != null) > 0;
        }

        public async Task<byte[]> GetContentAsync(string attachmentContentId)
        {
            var fileCache = await GetFileCacheLocationAsync(attachmentContentId);
            if (files.IsFileExists(fileCache))
            {
                return files.ReadAllBytes(fileCache);
            }

            var attachmentContentData = this.attachmentContentDataRepository.GetById(attachmentContentId);
            return attachmentContentData?.Content;
        }

        public async Task<byte[]> GetPreviewContentAsync(string attachmentContentId)
        {
            var fileCache = await GetFileCacheLocationAsync(attachmentContentId);
            if (files.IsFileExists(fileCache))
            {
                return files.ReadAllBytes(fileCache);
            }

            var attachmentPreviewContentData = this.attachmentPreviewContentDataRepository.GetById(attachmentContentId);
            return attachmentPreviewContentData?.PreviewContent 
                   ?? this.attachmentContentDataRepository.GetById(attachmentContentId)?.Content;
        }

        private async Task<string> GetFileCacheDirectoryAsync() =>
            Path.Combine(await pathUtils.GetRootDirectoryAsync(), "_attachments");

        public async Task<string> GetFileCacheLocationAsync(string attachmentId)
        {
            var attachmentContentMeta = this.GetMetadata(attachmentId);

            var cacheDirectory = await GetFileCacheDirectoryAsync();
            return Path.Combine(cacheDirectory, $"{attachmentId}{GetFileExtensionByMimeType(attachmentContentMeta?.ContentType)}");
        }

        private static string GetFileExtensionByMimeType(string contentType)
        {
            switch (contentType)
            {
                case "application/pdf":
                    return ".pdf";
                default:
                    return ".attachment";
            }
        }

        public async Task<IEnumerable<string>> EnumerateCacheAsync()
        {
            List<string> result = new List<string>();

            var cacheDirectory = await GetFileCacheDirectoryAsync();

            if (this.files.IsDirectoryExists(cacheDirectory))
            {
                foreach (var file in this.files.GetFilesInDirectory(cacheDirectory))
                {
                    result.Add(Path.GetFileNameWithoutExtension(file));
                }
            }

            return result;
        }
    }
}
