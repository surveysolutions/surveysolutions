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
            await this.attachmentContentDataRepository.StoreAsync(new AttachmentContentData()
            {
                Id = attachmentContent.Id,
                Content = attachmentContent.Content
            });
            await this.attachmentContentMetadataRepository.StoreAsync(new AttachmentContentMetadata()
            {
                ContentType = attachmentContent.ContentType,
                Id = attachmentContent.Id,
                Size = attachmentContent.Size,
            });
        }
        
        public async Task<AttachmentContentMetadata> GetMetadataAsync(string attachmentContentId)
        {
            var attachmentContent = await this.attachmentContentMetadataRepository.GetByIdAsync(attachmentContentId);
            return attachmentContent;
        }

        public async Task<bool> IsExistAsync(string attachmentContentId)
        {
            var attachmentContent = await this.attachmentContentMetadataRepository.GetByIdAsync(attachmentContentId);
            return attachmentContent != null;
        }

        public async Task<byte[]> GetContentAsync(string attachmentContentId)
        {
            var attachmentContentData = await this.attachmentContentDataRepository.GetByIdAsync(attachmentContentId);
            return attachmentContentData?.Content;
        }
    }
}