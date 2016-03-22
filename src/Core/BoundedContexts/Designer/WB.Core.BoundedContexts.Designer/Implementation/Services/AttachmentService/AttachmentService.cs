using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using WB.Core.BoundedContexts.Designer.Resources;
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
            var attachmentIdentifiers = this.GetAttachmentsByQuestionnaire(questionnaireId)
                .Select(attachment => new { attachment.ContentId, attachment.AttachmentId })
                .ToList();

            var attachmentContentIds = attachmentIdentifiers.Select(ai => ai.ContentId).ToList();

            var attachmentSizeByContent = this.attachmentContentStorage.Query(
                contents => contents.Select(content => new {ContentId = content.ContentId, Size = content.Size })
                    .Where(content => attachmentContentIds.Contains(content.ContentId)).ToList());

            return attachmentIdentifiers.Select(ai => new AttachmentSize()
            {
                AttachmentId = ai.AttachmentId,
                ContentId = ai.ContentId,
                Size = attachmentSizeByContent.SingleOrDefault(s => s.ContentId == ai.ContentId)?.Size ?? 0
            }).ToList();
        }

        public string GetAttachmentContentId(byte[] binaryContent)
        {
            using (var sha1Service = new SHA1CryptoServiceProvider())
            {
                return BitConverter.ToString(sha1Service.ComputeHash(binaryContent)).Replace("-", string.Empty);
            }
        }

        public void SaveContent(string contentId, string contentType, byte[] binaryContent)
        {
            var isContentExists = this.attachmentContentStorage.Query(
                contents => contents.Select(content => content.ContentId).Any(content => content == contentId));

            if (isContentExists) return;

            AttachmentDetails details = GetAttachmentDetails(binaryContent, contentType);

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
                attachment.ContentId = attachmentContentId;
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

        private static AttachmentDetails GetAttachmentDetails(byte[] binaryContent, string contentType)
        {
            if (contentType.StartsWith("image/"))
            {
                return GetImageAttachmentDetails(binaryContent);
            }

            throw new FormatException(ExceptionMessages.Attachments_Unsupported_content);
        }

        private static AttachmentDetails GetImageAttachmentDetails(byte[] binaryContent)
        {
            using (var stream = new MemoryStream(binaryContent))
            {
                try
                {
                    var image = Image.FromStream(stream);
                    return new AttachmentDetails
                    {
                        Height = image.Size.Height,
                        Width = image.Size.Width
                    };
                }
                catch (ArgumentException e)
                {
                    throw new FormatException(ExceptionMessages.Attachments_uploaded_file_is_not_image, e);
                }
            }
        }
    }
}