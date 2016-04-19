﻿using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Services.Infrastructure
{
    public class AttachmentsCleanupService
    {
        private readonly IInterviewerQuestionnaireAccessor questionnairesAccessor;
        private readonly IAsyncPlainStorage<AttachmentContentMetadata> attachmentContentMetadataRepository;
        private readonly IAsyncPlainStorage<AttachmentContentData> attachmentContentDataRepository;
        private readonly ILogger logger;

        protected AttachmentsCleanupService()
        {
        }

        public AttachmentsCleanupService(IInterviewerQuestionnaireAccessor questionnairesAccessor, 
            IAsyncPlainStorage<AttachmentContentMetadata> attachmentContentMetadataRepository,
            IAsyncPlainStorage<AttachmentContentData> attachmentContentDataRepository,
            ILogger logger)
        {
            this.questionnairesAccessor = questionnairesAccessor;
            this.attachmentContentMetadataRepository = attachmentContentMetadataRepository;
            this.attachmentContentDataRepository = attachmentContentDataRepository;
            this.logger = logger;
        }

        public virtual async Task RemovedOrphanedAttachments()
        {
            var contentMetadatas = this.attachmentContentMetadataRepository.LoadAll();

            foreach (var attachmentContentMetadata in contentMetadatas)
            {
                if (!this.questionnairesAccessor.IsAttachmentUsedAsync(attachmentContentMetadata.Id))
                {
                    await this.attachmentContentMetadataRepository.RemoveAsync(attachmentContentMetadata.Id);
                    await this.attachmentContentDataRepository.RemoveAsync(attachmentContentMetadata.Id);
                    this.logger.Info($"Removed attachment with Id {attachmentContentMetadata.Id}");
                }
            }
        }
    }
}