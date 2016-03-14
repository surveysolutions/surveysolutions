using System;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public class QuestionnaireAttachmentStorage : IQuestionnaireAttachmentStorage
    {
        private readonly IAsyncPlainStorage<AttachmentMetadata> attachmentMetadataRepository;
        private readonly IAsyncPlainStorage<AttachmentContent> attachmentContentRepository;

        public QuestionnaireAttachmentStorage(IAsyncPlainStorage<AttachmentMetadata> attachmentMetadataRepository,
            IAsyncPlainStorage<AttachmentContent> attachmentContentRepository)
        {
            this.attachmentMetadataRepository = attachmentMetadataRepository;
            this.attachmentContentRepository = attachmentContentRepository;
        }

        public async Task StoreAttachmentContentAsync(string attachmentId, byte[] attachmentData)
        {
            await this.attachmentContentRepository.StoreAsync(new AttachmentContent()
            {
                Id = attachmentId,
                Content = attachmentData
            });
        }

        public async Task StoreAsync(AttachmentMetadata attachmentMetadata)
        {
            await this.attachmentMetadataRepository.StoreAsync(attachmentMetadata);
        }
        
        public Task<AttachmentMetadata> GetAttachmentAsync(string attachmentId)
        {
            return Task.FromResult(this.attachmentMetadataRepository.GetById(attachmentId));
        }

        public Task<byte[]> GetAttachmentContentAsync(string attachmentContentId)
        {
            var attachmentContent = this.attachmentContentRepository.GetById(attachmentContentId);
            if (attachmentContent == null)
                return null;
            return Task.FromResult(attachmentContent.Content);
        }

        public Task<bool> IsExistAttachmentContentAsync(string attachmentContentId)
        {
            var attachmentContent = this.attachmentContentRepository.GetById(attachmentContentId);
            var fileExists = attachmentContent != null;
            return Task.FromResult(fileExists);
        }
    }
}