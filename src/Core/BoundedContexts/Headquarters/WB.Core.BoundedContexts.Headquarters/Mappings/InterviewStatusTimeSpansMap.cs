using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class InterviewStatusTimeSpansMap : ClassMapping<InterviewStatusTimeSpans>
    {
        public InterviewStatusTimeSpansMap()
        {
            this.Table("InterviewStatusTimeSpans");

            Id(x => x.InterviewId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });
            Property(x => x.QuestionnaireId);
            Property(x => x.QuestionnaireVersion);

            Set(x => x.TimeSpansBetweenStatuses, set => {
                    set.Key(key =>
                    {
                        key.Column(cm =>
                        {
                            cm.Name("InterviewId");
                            cm.Index("InterviewStatusTimeSpans_InterviewId");
                        });
                        key.ForeignKey("FK_InterviewStatusTimeSpans_TimeSpansBetweenStatuses");
                    });
                    set.Lazy(CollectionLazy.NoLazy);
                    set.Cascade(Cascade.All | Cascade.DeleteOrphans);
                },
                relation => relation.OneToMany());
        }
    }
}