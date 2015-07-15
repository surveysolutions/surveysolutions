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

            List(x => x.InterviewCommentedStatuses, listMap =>
            {
                listMap.Table("InterviewCommentedStatuses");
                listMap.Index(index => index.Column("Position"));
                listMap.Key(keyMap =>
                {
                    keyMap.Column(clm =>
                    {
                        clm.Name("InterviewId");
                        clm.Index("InterviewStatuseses_InterviewCommentedStatuses");
                    });
                });
                listMap.Cascade(Cascade.All | Cascade.DeleteOrphans);
            },
                rel =>
                {
                    rel.Component(cmp =>
                    {
                        cmp.Property(x => x.Id);
                        cmp.Property(x => x.SupervisorId);
                        cmp.Property(x => x.InterviewerId);
                        cmp.Property(x => x.StatusChangeOriginatorId);
                        cmp.Property(x => x.Timestamp);
                        cmp.Property(x => x.StatusChangeOriginatorName);
                        cmp.Property(x => x.Status);
                        cmp.Property(x => x.Comment);
                        cmp.Property(x => x.TimeSpanWithPreviousStatus);
                        cmp.Property(x => x.SupervisorName);
                        cmp.Property(x => x.InterviewerName);
                    });
                });
        }
    }
}