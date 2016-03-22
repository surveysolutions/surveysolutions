using System;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService
{
    public class AttachmentMeta
    {
        public virtual Guid AttachmentId { get; set; }
        public virtual Guid QuestionnaireId { get; set; }
        public virtual string ContentId { get; set; }
        public virtual string FileName { get; set; }
        public virtual DateTime LastUpdateDate { get; set; }
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

            this.Property(x => x.FileName);
            this.Property(x => x.LastUpdateDate);
            this.Property(x => x.QuestionnaireId, cm => cm.Index("AttachmentMeta_QuestionnaireId"));

            this.Property(x => x.ContentId, cm => cm.Index("AttachmentMeta_ContentId"));
        }
    }
}