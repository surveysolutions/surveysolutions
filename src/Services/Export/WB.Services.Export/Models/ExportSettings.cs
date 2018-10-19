using System;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Models
{
    public class ExportSettings
    {
        public DataExportFormat ExportFormat { get; set; }
        public QuestionnaireId QuestionnaireId { get; set; }
        public InterviewStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public TenantInfo Tenant { get; set; }

        public string NaturalId => $"{Tenant}${InterviewStatusString()}${ExportFormat}${this.QuestionnaireId}" +
                                   $"${this.FromDate?.ToString(@"YYYYMMDD") ?? "EMPTY FROM DATE"}" +
                                   $"${this.ToDate?.ToString(@"YYYYMMDD") ?? "EMPTY TO DATE"}";

        private string InterviewStatusString() => Status?.ToString() ?? "All";

        public override string ToString()
        {
            return
                $"{Tenant.Id} - {QuestionnaireId} {Status?.ToString() ?? "AllStatus"} {FromDate}-{ToDate}";
        }
    }
}
