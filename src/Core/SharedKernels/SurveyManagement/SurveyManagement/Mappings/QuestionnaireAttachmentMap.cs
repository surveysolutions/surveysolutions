using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    [PlainStorage]
    public class QuestionnaireAttachmentMap : ClassMapping<QuestionnaireAttachment>
    {
        public QuestionnaireAttachmentMap()
        {
            this.Id(x => x.AttachmentHash, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });

            this.Property(x => x.ContentType);
            this.Property(x => x.Content);
        }
    }
}