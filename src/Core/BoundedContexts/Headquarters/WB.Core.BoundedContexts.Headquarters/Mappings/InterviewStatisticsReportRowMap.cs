using NHibernate.Mapping.ByCode;
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
            Id(x => x.Id, m => m.Generator(Generators.Identity));

            Property(x => x.EntityId, pm => pm.Column("entity_id"));
            Property(x => x.RosterVector);

            Property(m => m.Answer, pm => pm.Type<Int64ArrayType>());
            Property(m => m.IsEnabled, pm => pm.Column("is_enabled"));
            Property(m => m.Type);

            ManyToOne(x => x.InterviewSummary, mto =>
            {
               mto.Column("interview_id");
                mto.PropertyRef(nameof(InterviewSummary.Id));
            });
            
            DynamicUpdate(true);
        }
    }
}
