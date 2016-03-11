using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Security.Cryptography;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService
{
    public class AttachmentService : IAttachmentService
    {
        private IPlainStorageAccessor<AttachmentContent> attachmentContentStorage => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<AttachmentContent>>();
        private IPlainStorageAccessor<AttachmentMeta> attachmentMetaStorage => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<AttachmentMeta>>();


        private readonly ISerializer serializer;

        public AttachmentService(ISerializer serializer)
        {
            this.serializer = serializer;
        }

        public void SaveAttachmentContent(Guid questionnaireId, Guid attachmentId, AttachmentType type, string contentType, byte[] binaryContent, string fileName)
        {
            var formattedAttachmentId = attachmentId.FormatGuid();
            var storedAttachmentMeta = this.attachmentMetaStorage.GetById(formattedAttachmentId);
            var hashOfBinaryContent = GetHash(binaryContent);
            var oldHashOfBinaryContent = "";

            if (storedAttachmentMeta != null)
            {
                var sameFileWasUploaded = hashOfBinaryContent == storedAttachmentMeta.AttachmentContentId;
                if (sameFileWasUploaded)
                    return;

                oldHashOfBinaryContent = storedAttachmentMeta.AttachmentContentId;
            }

            var attachmentTypeSpecificMeta = BuildAttachmentMeta(type, binaryContent, fileName);

            var countOfNewAttachmentContentReferences = this.attachmentMetaStorage.Query(_ => _.Count(x => x.AttachmentContentId == hashOfBinaryContent));
            if (countOfNewAttachmentContentReferences == 0)
            {
                var attachmentContent = new AttachmentContent
                {
                    AttachmentContentId = hashOfBinaryContent,
                    Content = binaryContent
                };

                this.attachmentContentStorage.Store(attachmentContent, hashOfBinaryContent);
            }

            var attachmentMeta = storedAttachmentMeta ?? new AttachmentMeta
            {
                AttachmentId = formattedAttachmentId,
                QuestionnaireId = questionnaireId.FormatGuid()
            };

            attachmentMeta.Type = type;
            attachmentMeta.ContentType = contentType;
            attachmentMeta.LastUpdateDate = DateTime.Now;
            attachmentMeta.Meta = attachmentTypeSpecificMeta;
            attachmentMeta.FileName = fileName;
            attachmentMeta.Size = binaryContent.LongLength;
            attachmentMeta.AttachmentContentId = hashOfBinaryContent;

            this.attachmentMetaStorage.Store(attachmentMeta, attachmentId);

            var attachmentHadContentBeforeThisUpload = !string.IsNullOrWhiteSpace(oldHashOfBinaryContent);
            if (attachmentHadContentBeforeThisUpload)
            {
                var countOfOldAttachmentContentReferences = this.attachmentMetaStorage.Query(_ => _.Count(x => x.AttachmentContentId == oldHashOfBinaryContent));
                if (countOfOldAttachmentContentReferences == 0)
                {
                    this.attachmentContentStorage.Remove(oldHashOfBinaryContent);
                }
            }
        }

        public void UpdateAttachmentName(Guid questionnaireId, Guid attachmentId, string name)
        {
            var formattedAttachmentId = attachmentId.FormatGuid();
            var attachmentMeta = this.attachmentMetaStorage.GetById(formattedAttachmentId) ?? new AttachmentMeta
            {
                AttachmentId = formattedAttachmentId,
                QuestionnaireId = questionnaireId.FormatGuid()
            };
            attachmentMeta.Name = name;
            this.attachmentMetaStorage.Store(attachmentMeta, formattedAttachmentId);
        }

        public void DeleteAttachment(Guid attachmentId)
        {
            var formattedAttachmentId = attachmentId.FormatGuid();
            try
            {
                var storedAttachmentMeta = this.attachmentMetaStorage.GetById(formattedAttachmentId);
            
                this.attachmentMetaStorage.Remove(formattedAttachmentId);

                if (storedAttachmentMeta != null)
                {
                    var countOfAttachmentContentReferences = this.attachmentMetaStorage.Query(_ => _.Count(x => x.AttachmentContentId == storedAttachmentMeta.AttachmentContentId));
                    if (countOfAttachmentContentReferences == 0)
                    {
                        this.attachmentContentStorage.Remove(storedAttachmentMeta.AttachmentContentId);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public QuestionnaireAttachment GetAttachment(Guid attachmentId)
        {
            var formattedAttachmentId = attachmentId.FormatGuid();
            var meta = this.attachmentMetaStorage.GetById(formattedAttachmentId);

            if (meta == null)
                return null;

            var content = attachmentContentStorage.GetById(meta.AttachmentContentId);

            return new QuestionnaireAttachment
            {
                AttachmentId = formattedAttachmentId,
                FileName = meta.FileName,
                Content = content.Content,
                AttachmentContentId = content.AttachmentContentId,
                ContentType = meta.ContentType
            };
        }

        public IEnumerable<QuestionnaireAttachmentMeta> GetBriefAttachmentsMetaForQuestionnaire(Guid questionnaireId)
        {
            var formattedQuestionnaireId = questionnaireId.FormatGuid();
            var attachmentsMeta = this.attachmentMetaStorage.Query(_ => _
                .Where(x => x.QuestionnaireId == formattedQuestionnaireId)
                .Select(x => new QuestionnaireAttachmentMeta
                {
                    AttachmentId = Guid.Parse(x.AttachmentId),
                    AttachmentContentId = x.AttachmentContentId,
                    ContentType = x.ContentType,
                })
                .ToList());
           
            return attachmentsMeta;
        }

        public IEnumerable<AttachmentView> GetAttachmentsForQuestionnaire(Guid questionnaireId)
        {
            var formattedQuestionnaireId = questionnaireId.FormatGuid();
            var attachmentsMeta = this.attachmentMetaStorage.Query(_ => _
                .Where(x => x.QuestionnaireId == formattedQuestionnaireId)
                .ToList());

            var attachmentsForQuestionnaire = attachmentsMeta.Select(x => new AttachmentView
            {
                ItemId = x.AttachmentId,
                Type = x.Type.ToString(),
                Name = x.Name,
                FileName = x.FileName,
                SizeInBytes = x.Size,
                LastUpdated = x.LastUpdateDate,
                Meta = ParseAttachmentMeta(x.Type, x.Meta)
            });

            return attachmentsForQuestionnaire;
        }

        public void CloneAttachmentMeta(Guid sourceAttachmentId, Guid newAttachmentId)
        {
            var formattedSourceAttachmentId = sourceAttachmentId.FormatGuid();
            var formattedNewAttachmentId = sourceAttachmentId.FormatGuid();

            var storedAttachmentMeta = this.attachmentMetaStorage.GetById(formattedSourceAttachmentId);
            var clonedAttachmentMeta = new AttachmentMeta
            {
                AttachmentId = formattedNewAttachmentId,
                QuestionnaireId = storedAttachmentMeta.QuestionnaireId,
                Name = storedAttachmentMeta.Name,
                FileName = storedAttachmentMeta.FileName,
                Size = storedAttachmentMeta.Size,
                Meta = storedAttachmentMeta.Meta,
                Type = storedAttachmentMeta.Type,
                ContentType = storedAttachmentMeta.ContentType,
                LastUpdateDate = storedAttachmentMeta.LastUpdateDate,
                AttachmentContentId = storedAttachmentMeta.AttachmentContentId,
            };
            this.attachmentMetaStorage.Store(clonedAttachmentMeta, formattedNewAttachmentId);
        }

        private string GetHash(byte[] binaryContent)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(binaryContent);
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        private object ParseAttachmentMeta(AttachmentType type, string meta)
        {
            if (type == AttachmentType.Image)
            {
                return this.serializer.Deserialize<ImageAttachmentMeta>(meta, TypeSerializationSettings.None);
            }
            return null;
        }

        private string BuildAttachmentMeta(AttachmentType type, byte[] binaryContent, string fileName)
        {
            if (type == AttachmentType.Image)
            {
                var meta = GetImageMeta(binaryContent, fileName);
                return this.serializer.Serialize(meta, TypeSerializationSettings.None);
            }
            return string.Empty;
        }

        public ImageAttachmentMeta GetImageMeta(byte[] binaryContent, string fileName)
        {
            using (var stream = new MemoryStream(binaryContent))
            {
                try
                {
                    var image = Image.FromStream(stream);
                    return new ImageAttachmentMeta
                    {
                        Height = image.Size.Height,
                        Width = image.Size.Height,
                        Format = image.RawFormat
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
