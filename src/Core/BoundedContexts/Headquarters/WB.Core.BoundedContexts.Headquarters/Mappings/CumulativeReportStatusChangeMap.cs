using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class CumulativeReportStatusChangeMap : ClassMapping<CumulativeReportStatusChange>
    {
        public CumulativeReportStatusChangeMap()
        {
            this.Id(_ => _.EntryId, mapper =>
            {
                mapper.Column("entryid");
                mapper.Generator(Generators.Assigned);
            });
            
            this.Property(_ => _.QuestionnaireIdentity, pm =>
            {
                pm.Column("questionnaireidentity");
                pm.Column(cm => cm.Index("CumulativeReportStatusChanges_QuestionnaireIdentity"));
            });
            this.Property(_ => _.InterviewId, pm =>
            {
                pm.Column("interviewid");
                pm.Column(cm => cm.Index("CumulativeReportStatusChanges_InterviewId"));
            });
            this.Property(_ => _.EventSequence, pm =>
            {
                pm.Column("eventsequence");
                pm.Column(cm => cm.Index("CumulativeReportStatusChanges_EventSequence"));
            });
            this.Property(_ => _.Date, pm =>
            {
                pm.Column("date");
                pm.Column(cm => cm.Index("CumulativeReportStatusChanges_Date"));
            });
            this.Property(_ => _.Status, pm => pm.Column("status"));
            this.Property(_ => _.ChangeValue, pm => pm.Column("changevalue"));
        }
    }
}
