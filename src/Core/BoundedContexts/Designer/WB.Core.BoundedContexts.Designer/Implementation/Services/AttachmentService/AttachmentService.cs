using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Practices.ServiceLocation;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;

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

            var attachmentTypeSpecificMeta = BuildAttachmentMeta(type, binaryContent);

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

        public IEnumerable<AttachmentView> GetAttachmentsForQuestionnaire(string questionnaireId)
        {
            var attachmentsMeta = this.attachmentMetaStorage.Query(_ => _
                .Where(x => x.QuestionnaireId == questionnaireId)
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

        public void CloneAttachmentMeta(Guid sourceAttachmentId)
        {
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

        private string BuildAttachmentMeta(AttachmentType type, byte[] binaryContent)
        {
            if (type == AttachmentType.Image)
            {
                var meta = GetImageMeta(binaryContent);
                return this.serializer.Serialize(meta, TypeSerializationSettings.None);
            }
            return string.Empty;
        }

        public ImageAttachmentMeta GetImageMeta(byte[] binaryContent)
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
                catch (ArgumentException)
                {
                    return null;
                }
            }
        }
    }

    public class QuestionnaireAttachment
    {
        public virtual string AttachmentId { get; set; }
        public virtual string FileName { get; set; }
        public virtual string ContentType { get; set; }
        public virtual string AttachmentContentId { get; set; }
        public virtual byte[] Content { get; set; }
    }

    public class ImageAttachmentMeta
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public ImageFormat Format { get; set; }
    }

    public class AttachmentMeta
    {
        public virtual string AttachmentId { get; set; }
        public virtual string QuestionnaireId { get; set; }
        public virtual string Name { get; set; }
        public virtual string FileName { get; set; }
        public virtual long Size { get; set; }
        public virtual string Meta { get; set; }
        public virtual AttachmentType Type { get; set; }
        public virtual string ContentType { get; set; }
        public virtual DateTime LastUpdateDate { get; set; }
        public virtual string AttachmentContentId { get; set; }
    }

    public class AttachmentContent
    {
        public virtual string AttachmentContentId { get; set; }
        public virtual byte[] Content { get; set; }
    }

   

    [PlainStorage]
    public class QuestionnaireAttachmentMetaMap : ClassMapping<AttachmentMeta>
    {
        public QuestionnaireAttachmentMetaMap()
        {
            Id(x => x.AttachmentId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });

            Property(x => x.Name);
            Property(x => x.FileName);
            Property(x => x.LastUpdateDate);
            Property(x => x.QuestionnaireId);
            Property(x => x.Size);

            Property(x => x.Meta);
            Property(x => x.Type);
            Property(x => x.AttachmentContentId);
        }
    }

    [PlainStorage]
    public class QuestionnaireAttachmentContentMap : ClassMapping<AttachmentContent>
    {
        public QuestionnaireAttachmentContentMap()
        {
            Id(x => x.AttachmentContentId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });

            Property(x => x.Content);
        }
    }
}
