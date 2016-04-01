using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    [PlainStorage]
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
            Property(x => x.QuestionnaireContentVersion);

            List(x => x.FeaturedQuestions, listMap =>
            {
                listMap.Table("FeaturedQuestions");
                listMap.Index(index => index.Column("Position"));
                listMap.Key(keyMap =>
                {
                    keyMap.Column(clm =>
                    {
                        clm.Name("QuestionnaireId");
                        clm.Index("QuestionnaireBrowseItems_FeaturedQuestions");
                    });
                });
                listMap.Cascade(Cascade.All | Cascade.DeleteOrphans);
            },
            rel =>
            {
                rel.Component(cmp =>
                {
                    cmp.Property(x => x.Id);
                    cmp.Property(x => x.Caption);
                    cmp.Property(x => x.Title);
                });
            });
        }
    }
}