using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Views.QuestionnaireAttachments;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    [PlainStorage]
    public class QuestionnaireAttachmentMap : ClassMapping<QuestionnaireAttachment>
    {
        public QuestionnaireAttachmentMap()
        {
            this.Id(x => x.Id, idMap =>
            {
                idMap.Generator(Generators.Guid);
                idMap.Column("Id");
            });

            this.Property(x => x.AttachmentId);
            this.Component(x => x.QuestionnairetIdentity, cm =>
            {
                cm.Property(c =>c.QuestionnaireId);
                cm.Property(c=>c.Version);
            });
            this.Property(x => x.AttachmentType);
            this.Property(x => x.AttachmentContentType);
        }
    }
}