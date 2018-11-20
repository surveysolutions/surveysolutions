using System;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService
{
    public class AttachmentContent
    {
        public virtual string ContentId { get; set; }
        public virtual byte[] Content { get; set; }
        public virtual long Size { get; set; }
        public virtual AttachmentDetails Details { get; set; }
        public virtual string ContentType { get; set; }

        public virtual bool IsImage() => this.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase);
        public virtual bool IsAudio() => this.ContentType.StartsWith("audio", StringComparison.OrdinalIgnoreCase);
        public virtual bool IsVideo() => this.ContentType.StartsWith("video", StringComparison.OrdinalIgnoreCase);
        public virtual bool IsPdf() => this.ContentType.StartsWith("application/pdf", StringComparison.OrdinalIgnoreCase);
    }

    [PlainStorage]
    public class QuestionnaireAttachmentContentMap : ClassMapping<AttachmentContent>
    {
        public QuestionnaireAttachmentContentMap()
        {
            this.Id(x => x.ContentId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });

            this.Property(x => x.Content, ptp => { ptp.Lazy(true);  });

            this.Property(x => x.Size);

            this.Property(x => x.ContentType);

            Component(x => x.Details, cmp =>
            {
                cmp.Property(x => x.Height, ptp => ptp.Column("AttachmentHeight"));
                cmp.Property(x => x.Width, ptp => ptp.Column("AttachmentWidth"));
                cmp.Property(x => x.Thumbnail, ptp => { ptp.Lazy(true); });
            });
        }
    }
}
