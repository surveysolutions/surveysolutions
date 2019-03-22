using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class InterviewStatisticsReportRowMap : ClassMapping<InterviewStatisticsReportRow>
    {
        public InterviewStatisticsReportRowMap()
        {
            Table("report_statistics");

            ComposedId(m =>
            {
                m.Property(x => x.InterviewId, pm => pm.Column("interview_id"));
                m.Property(x => x.EntityId, pm => pm.Column("entity_id"));
                m.Property(x => x.RosterVector);
            });

            Property(m => m.Answer, pm => pm.Type<Int64ArrayType>());
            Property(m => m.IsEnabled, pm => pm.Column("is_enabled"));
            Property(m => m.Type);
        }
    }
}
