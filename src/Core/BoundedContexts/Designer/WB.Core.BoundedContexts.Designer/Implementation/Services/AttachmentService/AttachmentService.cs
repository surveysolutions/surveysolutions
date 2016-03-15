using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService
{
    public class AttachmentService : IAttachmentService
    {
        private IPlainStorageAccessor<AttachmentContent> attachmentContentStorage => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<AttachmentContent>>();
        private IPlainStorageAccessor<AttachmentMeta> attachmentMetaStorage => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<AttachmentMeta>>();

        internal Func<MemoryStream, Image> ImageFromStream = stream => Image.FromStream(stream);

        public void SaveAttachmentContent(Guid questionnaireId, Guid attachmentId, AttachmentType type, string contentType, byte[] binaryContent, string fileName)
        {
            var formattedAttachmentId = attachmentId.FormatGuid();
            var storedAttachmentMeta = this.attachmentMetaStorage.GetById(formattedAttachmentId);
            var hashOfBinaryContent = GetHash(binaryContent);
            var oldHashOfBinaryContent = "";

            if (storedAttachmentMeta != null)
            {
                var sameFileWasUploaded = hashOfBinaryContent == storedAttachmentMeta.AttachmentContentHash;
                if (sameFileWasUploaded)
                    return;

                oldHashOfBinaryContent = storedAttachmentMeta.AttachmentContentHash;
            }

            var attachmentDetails = BuildAttachmentMeta(type, binaryContent, fileName);

            var countOfNewAttachmentContentReferences = this.attachmentMetaStorage.Query(_ => _.Count(x => x.AttachmentContentHash == hashOfBinaryContent));
            if (countOfNewAttachmentContentReferences == 0)
            {
                var attachmentContent = new AttachmentContent
                {
                    AttachmentContentHash = hashOfBinaryContent,
                    Content = binaryContent,
                    Type = type,
                    ContentType = contentType,
                    Details = attachmentDetails,
                    Size = binaryContent.LongLength
                };

                this.attachmentContentStorage.Store(attachmentContent, hashOfBinaryContent);
            }

            var attachmentMeta = storedAttachmentMeta ?? new AttachmentMeta
            {
                AttachmentId = formattedAttachmentId,
                QuestionnaireId = questionnaireId.FormatGuid()
            };
           
            attachmentMeta.LastUpdateDate = DateTime.UtcNow;
            attachmentMeta.FileName = fileName;
            attachmentMeta.AttachmentContentHash = hashOfBinaryContent;

            this.attachmentMetaStorage.Store(attachmentMeta, formattedAttachmentId);

            var attachmentHadContentBeforeThisUpload = !string.IsNullOrWhiteSpace(oldHashOfBinaryContent);
            if (attachmentHadContentBeforeThisUpload)
            {
                var countOfOldAttachmentContentReferences = this.attachmentMetaStorage.Query(_ => _.Count(x => x.AttachmentContentHash == oldHashOfBinaryContent));
                if (countOfOldAttachmentContentReferences == 0)
                {
                    this.attachmentContentStorage.Remove(oldHashOfBinaryContent);
                }
            }
        }

        public void DeleteAttachment(Guid attachmentId)
        {
            var formattedAttachmentId = attachmentId.FormatGuid();
            var storedAttachmentMeta = this.attachmentMetaStorage.GetById(formattedAttachmentId);
            
            this.attachmentMetaStorage.Remove(formattedAttachmentId);

            if (storedAttachmentMeta != null)
            {
                var countOfAttachmentContentReferences = this.attachmentMetaStorage.Query(_ => _.Count(x => x.AttachmentContentHash == storedAttachmentMeta.AttachmentContentHash));
                if (countOfAttachmentContentReferences == 0)
                {
                    this.attachmentContentStorage.Remove(storedAttachmentMeta.AttachmentContentHash);
                }
            }
        }

        public QuestionnaireAttachment GetAttachment(Guid attachmentId)
        {
            var formattedAttachmentId = attachmentId.FormatGuid();
            var meta = this.attachmentMetaStorage.GetById(formattedAttachmentId);

            if (meta == null)
                return null;

            var content = attachmentContentStorage.GetById(meta.AttachmentContentHash);

            return new QuestionnaireAttachment
            {
                AttachmentId = formattedAttachmentId,
                FileName = meta.FileName,
                Content = content.Content,
                AttachmentContentId = meta.AttachmentContentHash,
                ContentType = content.ContentType
            };
        }

        public IEnumerable<QuestionnaireAttachmentMeta> GetBriefAttachmentsMetaForQuestionnaire(Guid questionnaireId)
        {
            var formattedQuestionnaireId = questionnaireId.FormatGuid();
            var attachmentsMeta = this.attachmentMetaStorage.Query(_ => _
                .Where(x => x.QuestionnaireId == formattedQuestionnaireId)
                .Select(x => new 
                {
                    AttachmentId = Guid.Parse(x.AttachmentId),
                    AttachmentContentHash = x.AttachmentContentHash
                })
                .ToList());
           
            return attachmentsMeta.Select(x => new QuestionnaireAttachmentMeta
            {
                AttachmentId = x.AttachmentId,
                AttachmentContentHash = x.AttachmentContentHash
            });
        }

        public IEnumerable<AttachmentView> GetAttachmentsForQuestionnaire(Guid questionnaireId)
        {
            var formattedQuestionnaireId = questionnaireId.FormatGuid();
            var attachmentsMeta = this.attachmentMetaStorage.Query(_ => _
                .Where(x => x.QuestionnaireId == formattedQuestionnaireId)
                .ToList());

            var hashes = attachmentsMeta.Select(x => x.AttachmentContentHash).ToHashSet();

            var attachmentContentDetails = this.attachmentContentStorage.Query(_ => _
                .Where(x => hashes.Contains(x.AttachmentContentHash))
                .Select(x => new AttachmentContent
                {
                    AttachmentContentHash = x.AttachmentContentHash,
                    Size = x.Size,
                    Details = x.Details,
                    Type = x.Type,
                    ContentType = x.ContentType
                })
                .ToList());

            var attachmentsForQuestionnaire = from meta in attachmentsMeta
                                              join details in attachmentContentDetails on meta.AttachmentContentHash equals details.AttachmentContentHash
                                              select new AttachmentView
                                              {
                                                  ItemId = meta.AttachmentId,
                                                  FileName = meta.FileName,
                                                  LastUpdated = meta.LastUpdateDate,

                                                  Type = details.Type.ToString(),
                                                  SizeInBytes = details.Size,
                                                  Details = details.Details
                                              };

            return attachmentsForQuestionnaire;
        }

        public void CloneAttachmentMeta(Guid sourceAttachmentId, Guid newAttachmentId, Guid newQuestionnaireId)
        {
            var formattedSourceAttachmentId = sourceAttachmentId.FormatGuid();
            var formattedNewAttachmentId = newAttachmentId.FormatGuid();

            var storedAttachmentMeta = this.attachmentMetaStorage.GetById(formattedSourceAttachmentId);
            var clonedAttachmentMeta = new AttachmentMeta
            {
                AttachmentId = formattedNewAttachmentId,
                QuestionnaireId = newQuestionnaireId.FormatGuid(),
                FileName = storedAttachmentMeta.FileName,
                LastUpdateDate = storedAttachmentMeta.LastUpdateDate,
                AttachmentContentHash = storedAttachmentMeta.AttachmentContentHash,
            };
            this.attachmentMetaStorage.Store(clonedAttachmentMeta, formattedNewAttachmentId);
        }

        private string GetHash(byte[] binaryContent)
        {
            byte[] hash;
            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                hash = sha1.ComputeHash(binaryContent);
            }
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        private AttachmentDetails BuildAttachmentMeta(AttachmentType type, byte[] binaryContent, string fileName)
        {
            if (type == AttachmentType.Image)
            {
                return GetImageMeta(binaryContent, fileName);
            }
            return null;
        }

        public AttachmentDetails GetImageMeta(byte[] binaryContent, string fileName)
        {
            using (var stream = new MemoryStream(binaryContent))
            {
                try
                {
                    var image = ImageFromStream(stream);
                    return new AttachmentDetails
                    {
                        Height = image.Size.Height,
                        Width = image.Size.Width,
                        Format = new ImageFormatConverter().ConvertToString(image.RawFormat)
                    };
                }
                catch (ArgumentException e)
                {
                    throw new FormatException(string.Format(ExceptionMessages.Attachments_uploaded_file_is_not_image, fileName), e);
                }
            }
        }
    }
}