using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    [PlainStorage]
    public class AttachmentContentMap : ClassMapping<AttachmentContent>
    {
        public AttachmentContentMap()
        {
            this.Id(x => x.ContentHash, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });

            this.Property(x => x.ContentType);
            this.Property(x => x.Content);
            this.Property(x => x.FileName);
        }
    }
}
