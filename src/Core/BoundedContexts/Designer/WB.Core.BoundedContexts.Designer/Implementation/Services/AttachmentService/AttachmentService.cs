using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService
{
    public class AttachmentService : IAttachmentService
    {
        private readonly DesignerDbContext dbContext;
        private readonly IVideoConverter videoConverter;

        public AttachmentService(
            DesignerDbContext dbContext,
            IVideoConverter videoConverter)
        {
            this.dbContext = dbContext;
            this.videoConverter = videoConverter;
        }

        public void DeleteAllByQuestionnaireId(Guid questionnaireId)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                var questionnaireAttachments = this.dbContext.AttachmentMetas.Where(meta => meta.QuestionnaireId == questionnaireId).ToList();
                foreach (var questionnaireAttachment in questionnaireAttachments)
                {
                    this.dbContext.AttachmentMetas.Remove(questionnaireAttachment);
                    this.dbContext.SaveChanges();
                    var countOfAttachmentContentReferences = this.dbContext.AttachmentMetas.Count(meta => meta.ContentId == questionnaireAttachment.ContentId);
                    if (countOfAttachmentContentReferences == 0)
                    {
                        var content = this.dbContext.AttachmentContents.Find(questionnaireAttachment.ContentId);
                        if (content != null)
                        {
                            this.dbContext.AttachmentContents.Remove(content);
                        }
                    }
                    this.dbContext.SaveChanges();
                }
                transaction.Commit();
            }
        }

        public List<AttachmentMeta> GetAttachmentsByQuestionnaire(Guid questionnaireId)
        {
            return this.dbContext.AttachmentMetas.Where(attachment => attachment.QuestionnaireId == questionnaireId).ToList();
        }

        public AttachmentMeta? GetAttachmentMeta(Guid attachmentId)
        {
            return this.dbContext.AttachmentMetas.Find(attachmentId);
        }

        public AttachmentContent? GetContent(string contentId)
        {
            return this.dbContext.AttachmentContents.Find(contentId);
        }

        public List<AttachmentSize> GetAttachmentSizesByQuestionnaire(Guid questionnaireId)
        {
            var attachmentIdentifiers = this.GetAttachmentsByQuestionnaire(questionnaireId)
                .Select(attachment => new { attachment.ContentId, attachment.AttachmentId })
                .ToList();

            var attachmentContentIds = attachmentIdentifiers.Select(ai => ai.ContentId).ToList();

            var attachmentSizeByContent = this.dbContext.AttachmentContents
                    .Select(content => new { ContentId = content.ContentId, Size = content.Size })
                    .Where(content => attachmentContentIds.Contains(content.ContentId)).ToList();

            return attachmentIdentifiers.Select(ai => new AttachmentSize()
            {
                AttachmentId = ai.AttachmentId,
                ContentId = ai.ContentId,
                Size = attachmentSizeByContent.SingleOrDefault(s => s.ContentId == ai.ContentId)?.Size ?? 0
            }).ToList();
        }

        public string? GetAttachmentContentId(Guid attachmentId)
        {
            return this.dbContext.AttachmentMetas.Find(attachmentId)?.ContentId;
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
            var isContentExists = this.dbContext.AttachmentContents.Select(content => content.ContentId).Any(content => content == contentId);

            if (isContentExists) return;

            AttachmentDetails details = GetAttachmentDetails(binaryContent, contentType);

            this.dbContext.AttachmentContents.Add(new AttachmentContent
            {
                ContentId = contentId,
                ContentType = contentType,
                Size = binaryContent.Length,
                Details = details,
                Content = binaryContent
            });
        }

        public void SaveMeta(Guid attachmentId, Guid questionnaireId, string attachmentContentId, string fileName)
        {
            var attachment = this.dbContext.AttachmentMetas.Find(attachmentId);

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
                this.dbContext.AttachmentMetas.Add(attachment);
            }
            else
            {
                attachment.FileName = fileName;
                attachment.LastUpdateDate = DateTime.UtcNow;
                attachment.ContentId = attachmentContentId ?? attachment.ContentId;
                this.dbContext.AttachmentMetas.Update(attachment);
            }
        }

        public AttachmentContent? GetContentDetails(string attachmentContentId)
        {
            return this.dbContext.AttachmentContents.AsNoTracking().Select(content => new AttachmentContent
            {
                ContentId = content.ContentId,
                ContentType = content.ContentType,
                Size = content.Size,
                Details = content.Details
            }).FirstOrDefault(content => content.ContentId == attachmentContentId);
        }

        public void CloneMeta(Guid sourceAttachmentId, Guid newAttachmentId, Guid newQuestionnaireId)
        {
            var storedAttachmentMeta = this.dbContext.AttachmentMetas.Find(sourceAttachmentId);
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
            this.dbContext.AttachmentMetas.Add(clonedAttachmentMeta);
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
            try
            {
                using var image = SixLabors.ImageSharp.Image.Load(binaryContent);
                return new AttachmentDetails
                {
                    Height = image.Height,
                    Width = image.Width
                };
            }
            catch (Exception e)
            {
                throw new FormatException(ExceptionMessages.Attachments_uploaded_file_is_not_image, e);
            }
        }
    }
}
