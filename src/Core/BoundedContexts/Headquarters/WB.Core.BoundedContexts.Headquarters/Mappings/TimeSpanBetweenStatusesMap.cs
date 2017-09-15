using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
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
            Property(x => x.TimeSpanLong, clm =>
            {
                clm.Column("TimeSpan");
            });
            ManyToOne(x => x.InterviewSummary, mto =>
            {
                mto.Column("InterviewId");
                mto.Cascade(Cascade.None);
                mto.ForeignKey("FK_InterviewSummary_TimeSpansBetweenStatuses");
            });
        }
    }
}