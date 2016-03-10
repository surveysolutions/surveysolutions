using System;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService
{
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

    [PlainStorage]
    public class QuestionnaireAttachmentMetaMap : ClassMapping<AttachmentMeta>
    {
        public QuestionnaireAttachmentMetaMap()
        {
            this.Id(x => x.AttachmentId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });

            this.Property(x => x.Name);
            this.Property(x => x.FileName);
            this.Property(x => x.LastUpdateDate);
            this.Property(x => x.QuestionnaireId);
            this.Property(x => x.Size);

            this.Property(x => x.Meta);
            this.Property(x => x.Type);
            this.Property(x => x.AttachmentContentId);
        }
    }
}