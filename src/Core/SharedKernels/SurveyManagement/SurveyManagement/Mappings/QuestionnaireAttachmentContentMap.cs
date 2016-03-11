using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.QuestionnaireAttachments;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    [PlainStorage]
    public class QuestionnaireAttachmentContentMap : ClassMapping<QuestionnaireAttachmentContent>
    {
        public QuestionnaireAttachmentContentMap()
        {
            this.Id(x => x.AttachmentId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });

            this.Property(x => x.Content);
        }
    }
}