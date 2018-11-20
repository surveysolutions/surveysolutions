using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    [PlainStorage]
    public class QuestionnaireBrowseItemMap : ClassMapping<QuestionnaireBrowseItem>
    {
        public QuestionnaireBrowseItemMap()
        {
            this.Table("QuestionnaireBrowseItems");

            Id(x => x.Id);
            Property(x => x.QuestionnaireId);
            Property(x => x.Version);
            Property(x => x.CreationDate);
            Property(x => x.LastEntryDate);
            Property(x => x.ImportDate);
            Property(x => x.Title);
            Property(x => x.Variable);
            Property(x => x.IsPublic);
            Property(x => x.CreatedBy);
            Property(x => x.IsDeleted);
            Property(x => x.AllowCensusMode);
            Property(x => x.Disabled);
            Property(x => x.QuestionnaireContentVersion);
            Property(x => x.AllowAssignments);
            Property(x => x.AllowExportVariables);
            Property(x => x.DisabledBy);

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
                    keyMap.ForeignKey("FK_QuestionnaireBrowseItems_FeaturedQuestions");
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
