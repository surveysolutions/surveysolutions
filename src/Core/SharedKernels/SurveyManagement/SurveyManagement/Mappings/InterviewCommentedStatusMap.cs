using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    public class InterviewCommentedStatusMap : ClassMapping<InterviewCommentedStatus>
    {
        public InterviewCommentedStatusMap()
        {
            Id(x => x.Id, Idmap => Idmap.Generator(Generators.HighLow));
            Property(x => x.SupervisorId);
            Property(x => x.InterviewerId);
            Property(x => x.StatusChangeOriginatorId);
            Property(x => x.Timestamp);
            Property(x => x.StatusChangeOriginatorName);
            Property(x => x.Status);
            Property(x => x.Comment);
            Property(x => x.TimeSpanWithPreviousStatus);
            Property(x => x.SupervisorName);
            Property(x => x.InterviewerName);
            ManyToOne(x => x.InterviewStatuses, mto =>
            {
                mto.Index("InterviewStatuseses_InterviewCommentedStatuses");
                mto.Cascade(Cascade.None);
            });
        }
    }
}