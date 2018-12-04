using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class AttachmentContentService : IAttachmentContentService
    {
        private readonly IPlainStorageAccessor<AttachmentContent> attachmentContentStorage;

        public AttachmentContentService(IPlainStorageAccessor<AttachmentContent> attachmentContentStorage)
        {
            this.attachmentContentStorage = attachmentContentStorage;
        }

        public void SaveAttachmentContent(string contentHash, string contentType, string fileName, byte[] content)
        {
            var existingAttachment = this.attachmentContentStorage.GetById(contentHash);
            if (existingAttachment == null)
            {
                this.attachmentContentStorage.Store(new AttachmentContent
                {
                    ContentHash = contentHash,
                    ContentType = contentType,
                    Content = content,
                    FileName = fileName
                }, contentHash);
            }
        }

        public void DeleteAttachmentContent(string contentHash) => this.attachmentContentStorage.Remove(contentHash);

        public bool HasAttachmentContent(string contentHash) => this.attachmentContentStorage.Query(
            attachments => attachments.Select(content => content.ContentHash).Any(content => content == contentHash));

        public IEnumerable<AttachmentInfoView> GetAttachmentInfosByContentIds(IEnumerable<string> contentHashes)
        {
            var attachmentContentTypes = this.attachmentContentStorage.Query(_ => _
                .Where(x => contentHashes.Contains(x.ContentHash))
                .Select(x => new AttachmentInfoView(x.ContentHash, x.ContentType))
                .ToList());

            return attachmentContentTypes;
        }

        public AttachmentContent GetAttachmentContent(string contentHash) => this.attachmentContentStorage.GetById(contentHash);
    }
}
