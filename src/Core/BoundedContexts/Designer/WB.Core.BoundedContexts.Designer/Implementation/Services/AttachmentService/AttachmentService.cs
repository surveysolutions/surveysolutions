using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService
{
    public class AttachmentService : IAttachmentService
    {
        private readonly IPlainStorageAccessor<AttachmentContent> attachmentContentStorage;
        private readonly IPlainStorageAccessor<AttachmentMeta> attachmentMetaStorage;
        
        public AttachmentService(
            IPlainStorageAccessor<AttachmentContent> attachmentContentStorage,
            IPlainStorageAccessor<AttachmentMeta> attachmentMetaStorage)
        {
            this.attachmentContentStorage = attachmentContentStorage;
            this.attachmentMetaStorage = attachmentMetaStorage;
        }
        
        public void Delete(Guid attachmentId)
        {
            var dbAttachment = this.attachmentMetaStorage.GetById(attachmentId);

            if (dbAttachment != null)
            {
                this.attachmentMetaStorage.Remove(attachmentId);

                var countOfAttachmentContentReferences = this.attachmentMetaStorage.Query(metas => metas.Count(meta => meta.ContentId == dbAttachment.ContentId));
                if (countOfAttachmentContentReferences == 0)
                {
                    this.attachmentContentStorage.Remove(dbAttachment.ContentId);
                }
            }
        }

        public List<AttachmentMeta> GetAttachmentsByQuestionnaire(Guid questionnaireId)
        {
            return this.attachmentMetaStorage.Query(
                attachments => attachments.Where(attachment => attachment.QuestionnaireId == questionnaireId).ToList());
        }

        public QuestionnaireAttachment GetAttachment(Guid attachmentId)
        {
            var attachment = this.attachmentMetaStorage.GetById(attachmentId);
            var attachmentContent = this.attachmentContentStorage.GetById(attachment.ContentId);

            return new QuestionnaireAttachment
            {
                AttachmentId = attachment.AttachmentId.FormatGuid(),
                AttachmentContentId = attachment.ContentId,
                FileName = attachment.FileName,
                ContentType = attachmentContent.ContentType,
                Content = attachmentContent.Content
            };
        }

        public AttachmentContent GetContent(string contentId)
        {
            return this.attachmentContentStorage.GetById(contentId);
        }

        public List<AttachmentSize> GetAttachmentSizesByQuestionnaire(Guid questionnaireId)
        {
            var attachmentIds = this.GetAttachmentsByQuestionnaire(questionnaireId).Select(attachment => attachment.ContentId);

            return this.attachmentContentStorage.Query(
                contents => contents.Select(content => new AttachmentSize {ContentId = content.ContentId, Size = content.Size})
                        .Where(content => attachmentIds.Contains(content.ContentId)).ToList());
        }

        public void SaveContent(string contentId, string contentType, byte[] binaryContent, AttachmentDetails details)
        {
            var isContentExists = this.attachmentContentStorage.Query(
                contents => contents.Select(content => content.ContentId).Any(content => content == contentId));

            if (isContentExists) return;

            this.attachmentContentStorage.Store(new AttachmentContent
            {
                ContentId = contentId,
                ContentType = contentType,
                Size = binaryContent.Length,
                Details = details,
                Content = binaryContent
            }, contentId);
        }

        public void SaveMeta(Guid attachmentId, Guid questionnaireId, string attachmentContentId, string fileName)
        {
            var attachment = this.attachmentMetaStorage.GetById(attachmentId);

            if (attachment == null)
            {
                this.attachmentMetaStorage.Store(new AttachmentMeta
                {
                    AttachmentId = attachmentId,
                    QuestionnaireId = questionnaireId,
                    ContentId = attachmentContentId,
                    FileName = fileName,
                    LastUpdateDate = DateTime.UtcNow
                }, attachmentId);
            }
            else
            {
                attachment.QuestionnaireId = questionnaireId;
                attachment.FileName = fileName;
                attachment.LastUpdateDate = DateTime.UtcNow;
                this.attachmentMetaStorage.Store(attachment, attachment.AttachmentId);
            }
        }

        public AttachmentContentView GetContentDetails(string attachmentContentId)
        {
            return this.attachmentContentStorage.Query(contents=>contents.Select(content=>new AttachmentContentView
            {
                ContentId = content.ContentId,
                Type = content.ContentType,
                Size = content.Size,
                Details = content.Details
            }).FirstOrDefault(content=>content.ContentId == attachmentContentId));
        }

        public void CloneMeta(Guid sourceAttachmentId, Guid newAttachmentId, Guid newQuestionnaireId)
        {
            var storedAttachmentMeta = this.attachmentMetaStorage.GetById(sourceAttachmentId);
            var clonedAttachmentMeta = new AttachmentMeta
            {
                AttachmentId = newAttachmentId,
                QuestionnaireId = newQuestionnaireId,
                FileName = storedAttachmentMeta.FileName,
                LastUpdateDate = storedAttachmentMeta.LastUpdateDate,
                ContentId = storedAttachmentMeta.ContentId,
            };
            this.attachmentMetaStorage.Store(clonedAttachmentMeta, newAttachmentId);
        }
    }
}