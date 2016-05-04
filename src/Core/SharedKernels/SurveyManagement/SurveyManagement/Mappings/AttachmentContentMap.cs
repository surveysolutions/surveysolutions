using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
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
        }
    }
}