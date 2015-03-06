using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    public class QuestionnaireBrowseItemMap : ClassMapping<QuestionnaireBrowseItem>
    {
        public QuestionnaireBrowseItemMap()
        {
            Table("QuestionnaireBrowseItems");

            Id(x => x.Id);
            Property(x => x.QuestionnaireId);
            Property(x => x.Version);
            Property(x => x.CreationDate);
            Property(x => x.LastEntryDate);
            Property(x => x.Title);
            Property(x => x.IsPublic);
            Property(x => x.CreatedBy);
            Property(x => x.IsDeleted);
            Property(x => x.AllowCensusMode);
            Property(x => x.Disabled);
        }
    }
}