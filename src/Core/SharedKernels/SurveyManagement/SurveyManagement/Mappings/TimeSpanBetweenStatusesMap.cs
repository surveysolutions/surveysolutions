using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    public class TimeSpanBetweenStatusesMap : ClassMapping<TimeSpanBetweenStatuses>
    {
        public TimeSpanBetweenStatusesMap()
        {
            Id(x => x.Id, Idmap => Idmap.Generator(Generators.HighLow));
            Property(x => x.SupervisorId);
            Property(x => x.SupervisorName);
            Property(x => x.InterviewerId);
            Property(x => x.InterviewerName);
            Property(x => x.BeginStatus);
            Property(x => x.EndStatus);
            Property(x => x.EndStatusTimestamp);
            Property(x => x.TimeSpan);
            ManyToOne(x => x.InterviewStatusTimeSpans, mto =>
            {
                mto.Index("InterviewStatusTimeSpans_TimeSpansBetweenStatuses");
                mto.Cascade(Cascade.None);
            });
        }
    }
}