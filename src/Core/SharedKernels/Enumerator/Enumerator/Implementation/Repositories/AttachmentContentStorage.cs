﻿using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public class AttachmentContentStorage : IAttachmentContentStorage
    {
        private readonly IAsyncPlainStorage<AttachmentContentMetadata> attachmentContentMetadataRepository;
        private readonly IAsyncPlainStorage<AttachmentContentData> attachmentContentDataRepository;

        public AttachmentContentStorage(IAsyncPlainStorage<AttachmentContentMetadata> attachmentContentMetadataRepository,
            IAsyncPlainStorage<AttachmentContentData> attachmentContentDataRepository)
        {
            this.attachmentContentMetadataRepository = attachmentContentMetadataRepository;
            this.attachmentContentDataRepository = attachmentContentDataRepository;
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
    }
}