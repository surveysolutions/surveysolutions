using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class InterviewReportAnswerMap : ClassMapping<InterviewReportAnswer>
    {
        public InterviewReportAnswerMap()
        {
            Table("interview_report_answers");

            Id(x => x.Id, map => map.Generator(Generators.Identity));

            ManyToOne(x => x.InterviewSummary, mto => mto.Column("interview_id"));

            Property(x => x.Value, col => col.Column("value"));
            Property(x => x.AnswerCode, col => col.Column("answer_code"));
            Property(x => x.ValueLowerCase, col => col.Column("value_lower_case"));
            Property(x => x.IsEnabled, col => col.Column("enabled"));

            ManyToOne(x => x.Entity, mtm =>
            {
                mtm.Lazy(LazyRelation.Proxy);
                mtm.Column("entity_id");
            });
        }
    }
}
