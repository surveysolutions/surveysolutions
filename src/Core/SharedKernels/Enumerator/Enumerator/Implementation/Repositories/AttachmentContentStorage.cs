using System;
using System.IO;
using System.Linq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Api;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public class AttachmentContentStorage : IAttachmentContentStorage
    {
        private readonly IPlainStorage<AttachmentContentMetadata> attachmentContentMetadataRepository;
        private readonly IPlainStorage<AttachmentContentData> attachmentContentDataRepository;
        private readonly IFileSystemAccessor files;

        public AttachmentContentStorage(
            IPlainStorage<AttachmentContentMetadata> attachmentContentMetadataRepository,
            IPlainStorage<AttachmentContentData> attachmentContentDataRepository,
            IFileSystemAccessor files)
        {
            this.attachmentContentMetadataRepository = attachmentContentMetadataRepository;
            this.attachmentContentDataRepository = attachmentContentDataRepository;
            this.files = files;
        }

        public void Store(AttachmentContent attachmentContent)
        {
            this.attachmentContentDataRepository.Store(new AttachmentContentData
            {
                Id = attachmentContent.Id,
                Content = attachmentContent.Content
            });

            this.attachmentContentMetadataRepository.Store(new AttachmentContentMetadata
            {
                ContentType = attachmentContent.ContentType,
                Id = attachmentContent.Id,
                Size = attachmentContent.Size,
            });

            // storing video files in filesystem for video playback
            if (attachmentContent.IsVideo() || attachmentContent.IsPdf())
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
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "_attachments");

        public string GetFileCacheLocation(string attachmentId) 
            => Path.Combine(FileCacheDirectory, attachmentId);
    }
}
