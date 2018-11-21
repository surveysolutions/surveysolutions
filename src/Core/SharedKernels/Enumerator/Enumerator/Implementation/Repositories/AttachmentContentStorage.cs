using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Api;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public class AttachmentContentStorage : IAttachmentContentStorage
    {
        private readonly IPlainStorage<AttachmentContentMetadata> attachmentContentMetadataRepository;
        private readonly IPlainStorage<AttachmentContentData> attachmentContentDataRepository;
        private readonly IPathUtils pathUtils;
        private readonly IFileSystemAccessor files;

        public AttachmentContentStorage(
            IPlainStorage<AttachmentContentMetadata> attachmentContentMetadataRepository,
            IPlainStorage<AttachmentContentData> attachmentContentDataRepository,
            IPathUtils pathUtils,
            IFileSystemAccessor files)
        {
            this.attachmentContentMetadataRepository = attachmentContentMetadataRepository;
            this.attachmentContentDataRepository = attachmentContentDataRepository;
            this.pathUtils = pathUtils;
            this.files = files;
        }

        public void Store(AttachmentContent attachmentContent)
        {
            this.attachmentContentMetadataRepository.Store(new AttachmentContentMetadata
            {
                ContentType = attachmentContent.ContentType,
                Id = attachmentContent.Id,
                Size = attachmentContent.Size,
            });

            var storeInFileSystem = IsStoredInFileSystem(attachmentContent);
            if (!storeInFileSystem)
            {
                this.attachmentContentDataRepository.Store(new AttachmentContentData
                {
                    Id = attachmentContent.Id,
                    Content = attachmentContent.Content
                });
            }
            else
            {
                var fileCache = GetFileCacheLocation(attachmentContent.Id);

                if (!files.IsFileExists(fileCache))
                {
                    if (!files.IsDirectoryExists(FileCacheDirectory))
                    {
                        files.CreateDirectory(FileCacheDirectory);
                    }

                    files.WriteAllBytes(fileCache, attachmentContent.Content);
                }
            }
        }

        private bool IsStoredInFileSystem(AttachmentContent attachmentContent)
        {
            return attachmentContent.IsVideo() || attachmentContent.IsPdf() || attachmentContent.IsAudio();
        }

        public void Remove(string attachmentContentId)
        {
            this.attachmentContentMetadataRepository.Remove(attachmentContentId);
            this.attachmentContentDataRepository.Remove(attachmentContentId);

            var fileCache = GetFileCacheLocation(attachmentContentId);
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

        public bool Exists(string attachmentContentId)
        {
            var attachmentContent = this.attachmentContentMetadataRepository.Count(x => x.Id == attachmentContentId);
            return attachmentContent > 0;
        }

        public byte[] GetContent(string attachmentContentId)
        {
            var attachmentContentData = this.attachmentContentDataRepository.GetById(attachmentContentId);
            return attachmentContentData?.Content;
        }

        private string FileCacheDirectory =>
            Path.Combine(pathUtils.GetRootDirectory(), "_attachments");

        public string GetFileCacheLocation(string attachmentId)
        {
            var attachmentContentMeta = this.GetMetadata(attachmentId);

            return Path.Combine(FileCacheDirectory, $"{attachmentId}{GetFileExtensionByMimeType(attachmentContentMeta.ContentType)}");
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

        public IEnumerable<string> EnumerateCache()
        {
            if (this.files.IsDirectoryExists(FileCacheDirectory))
            {
                foreach (var file in this.files.GetFilesInDirectory(FileCacheDirectory))
                {
                    yield return Path.GetFileNameWithoutExtension(file);
                }
            }
        }
    }
}
