using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    public class CumulativeReportStatusChangeMap : ClassMapping<CumulativeReportStatusChange>
    {
        public CumulativeReportStatusChangeMap()
        {
            this.Id(_ => _.EntryId, mapper => mapper.Generator(Generators.Assigned));

            this.Property(_ => _.QuestionnaireId);
            this.Property(_ => _.QuestionnaireVersion);
            this.Property(_ => _.Date);
            this.Property(_ => _.Status);
            this.Property(_ => _.ChangeValue);
        }
    }
}