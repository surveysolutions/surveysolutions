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
        public virtual DateTime LastUpdateDate { get; set; }
        public virtual string AttachmentContentHash { get; set; }
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
            this.Property(x => x.QuestionnaireId, cm => cm.Index("AttachmentMeta_QuestionnaireId"));

            this.Property(x => x.AttachmentContentHash, cm => cm.Index("AttachmentMeta_AttachmentContentHash"));
        }
    }
}