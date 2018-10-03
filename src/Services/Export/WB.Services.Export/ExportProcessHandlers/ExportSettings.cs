using System;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.ExportProcessHandlers
{
    public class ExportSettings
    {
        public QuestionnaireId QuestionnaireId { get; set; }
        public InterviewStatus? InterviewStatus { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string ExportTempDirectory { get; set; }
        public TenantInfo Tenant { get; set; }
        public string ArchiveName { get; set; }

        public override string ToString()
        {
            return
                $"{Tenant.Id} - {ArchiveName} {QuestionnaireId} {InterviewStatus?.ToString() ?? "AllStatus"} {FromDate}-{ToDate}";
        }
    }
}
