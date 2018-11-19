using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService
{
    public class AttachmentService : IAttachmentService
    {
        private readonly IPlainStorageAccessor<AttachmentContent> attachmentContentStorage;
        private readonly IPlainStorageAccessor<AttachmentMeta> attachmentMetaStorage;
        private readonly IVideoConverter videoConverter;

        public AttachmentService(
            IPlainStorageAccessor<AttachmentContent> attachmentContentStorage,
            IPlainStorageAccessor<AttachmentMeta> attachmentMetaStorage,
            IVideoConverter videoConverter)
        {
            this.attachmentContentStorage = attachmentContentStorage;
            this.attachmentMetaStorage = attachmentMetaStorage;
            this.videoConverter = videoConverter;
        }
        
        public void DeleteAllByQuestionnaireId(Guid questionnaireId)
        {
            var questionnaireAttachments = this.attachmentMetaStorage.Query(metas => metas.Where(meta => meta.QuestionnaireId == questionnaireId)).ToList();
            foreach (var questionnaireAttachment in questionnaireAttachments)
            {
                this.attachmentMetaStorage.Remove(questionnaireAttachment.AttachmentId);

                var countOfAttachmentContentReferences = this.attachmentMetaStorage.Query(metas => metas.Count(meta => meta.ContentId == questionnaireAttachment.ContentId));
                if (countOfAttachmentContentReferences == 0)
                {
                    this.attachmentContentStorage.Remove(questionnaireAttachment.ContentId);
                }
            }
        }

        public List<AttachmentMeta> GetAttachmentsByQuestionnaire(Guid questionnaireId)
        {
            return this.attachmentMetaStorage.Query(
                attachments => attachments.Where(attachment => attachment.QuestionnaireId == questionnaireId).ToList());
        }

        public AttachmentMeta GetAttachmentMeta(Guid attachmentId)
        {
            return this.attachmentMetaStorage.GetById(attachmentId);
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

        public string GetAttachmentContentId(Guid attachmentId)
        {
            return this.attachmentMetaStorage.GetById(attachmentId)?.ContentId;
        }

        public string CreateAttachmentContentId(byte[] binaryContent)
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
               attachment = new AttachmentMeta
                {
                    AttachmentId = attachmentId,
                    QuestionnaireId = questionnaireId,
                    ContentId = attachmentContentId,
                    FileName = fileName,
                    LastUpdateDate = DateTime.UtcNow
                };
            }
            else
            {
                attachment.FileName = fileName;
                attachment.LastUpdateDate = DateTime.UtcNow;
                attachment.ContentId = attachmentContentId ?? attachment.ContentId;

            }
            this.attachmentMetaStorage.Store(attachment, attachment.AttachmentId);
        }

        public AttachmentContent GetContentDetails(string attachmentContentId)
        {
            return this.attachmentContentStorage.Query(contents=>contents.Select(content=>new AttachmentContent
            {
                ContentId = content.ContentId,
                ContentType = content.ContentType,
                Size = content.Size,
                Details = content.Details
            }).FirstOrDefault(content=>content.ContentId == attachmentContentId));
        }

        public void CloneMeta(Guid sourceAttachmentId, Guid newAttachmentId, Guid newQuestionnaireId)
        {
            var storedAttachmentMeta = this.attachmentMetaStorage.GetById(sourceAttachmentId);
            if (storedAttachmentMeta == null)
            {
                throw new ArgumentException(string.Format(ExceptionMessages.AttachmentIdIsMissing, sourceAttachmentId), nameof(sourceAttachmentId));
            }

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

        private AttachmentDetails GetAttachmentDetails(byte[] binaryContent, string contentType)
        {
            if (contentType.StartsWith("image/"))
                return GetImageAttachmentDetails(binaryContent);

            if (contentType.StartsWith("audio/"))
                return new AttachmentDetails();

            if (contentType.StartsWith("application/pdf"))
                return new AttachmentDetails();

            if (contentType.StartsWith("video/"))
            {
                var thumbnail = this.videoConverter.CreateThumbnail(binaryContent);
                var details = GetImageAttachmentDetails(thumbnail);
                details.Thumbnail = thumbnail;

                return details;
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
