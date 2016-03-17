using System;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
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

        public async Task StoreAsync(AttachmentContent attachmentContent)
        {
            await this.attachmentContentMetadataRepository.StoreAsync(new AttachmentContentMetadata()
            {
                ContentType = attachmentContent.ContentType,
                Id = attachmentContent.Id,
                Size = attachmentContent.Size,
            });
            await this.attachmentContentDataRepository.StoreAsync(new AttachmentContentData()
                    {
                        Id = attachmentContent.Id,
                        Content = attachmentContent.Content
            });
        }
        
        public Task<AttachmentContentMetadata> GetMetadataAsync(string attachmentContentId)
        {
            var attachmentContent = this.attachmentContentMetadataRepository.GetById(attachmentContentId);
            if (attachmentContent == null)
                return null;
            return Task.FromResult(attachmentContent);
        }

        public Task<bool> IsExistAsync(string attachmentContentId)
        {
            var attachmentContent = this.attachmentContentMetadataRepository.GetById(attachmentContentId);
            var fileExists = attachmentContent != null;
            return Task.FromResult(fileExists);
        }

        public Task<byte[]> GetContentAsync(string attachmentContentId)
        {
            var attachmentContentData = this.attachmentContentDataRepository.GetById(attachmentContentId);
            if (attachmentContentData == null)
                return null;

            return Task.FromResult(attachmentContentData.Content);
        }
    }
}