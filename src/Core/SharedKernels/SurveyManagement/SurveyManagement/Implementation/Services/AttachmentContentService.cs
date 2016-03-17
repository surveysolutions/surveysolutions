using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    public class AttachmentContentService : IAttachmentContentService
    {
        private readonly IPlainStorageAccessor<AttachmentContent> attachmentContentStorage;

        public AttachmentContentService(IPlainStorageAccessor<AttachmentContent> attachmentContentStorage)
        {
            this.attachmentContentStorage = attachmentContentStorage;
        }

        public void SaveAttachmentContent(string contentHash, string contentType, byte[] content)
            => this.attachmentContentStorage.Store(new AttachmentContent
            {
                ContentHash = contentHash,
                ContentType = contentType,
                Content = content
            }, contentHash);

        public void DeleteAttachmentContent(string contentHash) => this.attachmentContentStorage.Remove(contentHash);

        public bool HasAttachmentContent(string contentHash) => this.attachmentContentStorage.Query(
            attachments => attachments.Select(content => content.ContentHash).Any(content => content == contentHash));

        public Dictionary<string, AttachmentInfoView> GetContentTypes(HashSet<string> contentHashes)
        {
            var attachmentContentTypes = this.attachmentContentStorage.Query(_ => _
                .Where(x => contentHashes.Contains(x.ContentHash))
                .Select(x => new { x.ContentHash, x.ContentType})
                .Distinct()
                .ToDictionary(x => x.ContentHash, x => new AttachmentInfoView(x.ContentHash, x.ContentType)));

            return attachmentContentTypes;
        }

        public AttachmentContent GetAttachmentContent(string contentHash) => this.attachmentContentStorage.GetById(contentHash);
    }
}
