using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    public class QuestionnaireFeedEntryMap : ClassMapping<QuestionnaireFeedEntry>
    {
        public QuestionnaireFeedEntryMap()
        {
            this.Id(x => x.EntryId, mapper => mapper.Generator(Generators.Assigned));

            this.Property(x => x.QuestionnaireId);
            this.Property(x => x.QuestionnaireVersion);
            this.Property(x => x.EntryType);
            this.Property(x => x.Timestamp);
        }
    }
}