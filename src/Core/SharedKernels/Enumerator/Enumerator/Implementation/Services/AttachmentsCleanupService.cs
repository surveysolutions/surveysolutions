using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public interface IAttachmentsCleanupService
    {
        void RemovedOrphanedAttachments();
    }

    public class AttachmentsCleanupService : IAttachmentsCleanupService
    {
        private readonly IInterviewerQuestionnaireAccessor questionnairesAccessor;
        private readonly IPlainStorage<AttachmentContentMetadata> attachmentContentMetadataRepository;
        private readonly IAttachmentContentStorage attachmentContentStorage;
        private readonly ILogger logger;

        protected AttachmentsCleanupService()
        {
        }

        public AttachmentsCleanupService(IInterviewerQuestionnaireAccessor questionnairesAccessor,
            IPlainStorage<AttachmentContentMetadata> attachmentContentMetadataRepository,
            IAttachmentContentStorage attachmentContentStorage,
            ILogger logger)
        {
            this.questionnairesAccessor = questionnairesAccessor;
            this.attachmentContentMetadataRepository = attachmentContentMetadataRepository;
            this.logger = logger;
            this.attachmentContentStorage = attachmentContentStorage;
        }

        public virtual void RemovedOrphanedAttachments()
        {
            var contentMetadatas = this.attachmentContentMetadataRepository.LoadAll();

            var questionnaireDocuments = this.questionnairesAccessor.LoadAll()
                .Select(q => q.QuestionnaireDocument) // to make sure that we deserialize document only once
                .Where(qd => !qd.IsDeleted);

            var contentIdLookup = new HashSet<string>(questionnaireDocuments
                .SelectMany(qd => qd.Attachments.Select(a => a.ContentId)));

            foreach (var attachmentContentMetadata in contentMetadatas)
            {
                if(!contentIdLookup.Contains(attachmentContentMetadata.Id))
                {
                    this.attachmentContentStorage.Remove(attachmentContentMetadata.Id);
                    this.logger.Info($"Removed attachment with Id {attachmentContentMetadata.Id}");
                }
            }
        }
    }
}
