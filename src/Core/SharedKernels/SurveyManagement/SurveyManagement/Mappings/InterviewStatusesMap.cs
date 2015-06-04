using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    public class InterviewStatusesMap : ClassMapping<InterviewStatuses>
    {
        public InterviewStatusesMap()
        {
            Table("InterviewStatuses");

            Id(x => x.InterviewId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });
            Property(x => x.QuestionnaireId);
            Property(x => x.QuestionnaireVersion);

            Set(x => x.InterviewCommentedStatuses, set =>
            {
                set.Key(key => key.Column("InterviewId"));
                set.Lazy(CollectionLazy.NoLazy);
                set.Cascade(Cascade.All | Cascade.DeleteOrphans);
            },
                relation => relation.OneToMany());
        }
    }
}