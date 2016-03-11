using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService
{
    public class AttachmentContent
    {
        public virtual string AttachmentContentId { get; set; }
        public virtual byte[] Content { get; set; }
    }

    [PlainStorage]
    public class QuestionnaireAttachmentContentMap : ClassMapping<AttachmentContent>
    {
        public QuestionnaireAttachmentContentMap()
        {
            this.Id(x => x.AttachmentContentId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });

            this.Property(x => x.Content);
        }
    }
}