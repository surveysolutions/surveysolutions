using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    public class InterviewStatusTimeSpansMap : ClassMapping<InterviewStatusTimeSpans>
    {
        public InterviewStatusTimeSpansMap()
        {
            Table("InterviewStatusTimeSpans");

            Id(x => x.InterviewId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });
            Property(x => x.QuestionnaireId);
            Property(x => x.QuestionnaireVersion);

            Set(x => x.TimeSpansBetweenStatuses, set =>
            {
                set.Key(key => key.Column("InterviewId"));
                set.Lazy(CollectionLazy.NoLazy);
                set.Cascade(Cascade.All | Cascade.DeleteOrphans);
            },
                relation => relation.OneToMany());
        }
    }
}