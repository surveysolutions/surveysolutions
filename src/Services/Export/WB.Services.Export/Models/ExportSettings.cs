using System;
using WB.Services.Infrastructure.Tenant;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Models
{
    public class ExportSettings
    {
        public ExportSettings(
            DataExportFormat exportFormat, 
            QuestionnaireIdentity questionnaireId, 
            TenantInfo tenant,
            InterviewStatus? status = null,
            DateTime? fromDate = null, 
            DateTime? toDate = null, 
            Guid? translation = null, 
            bool? includeMeta = null,
            long? jobId = null)
        {
            JobId = jobId;
            ExportFormat = exportFormat;
            QuestionnaireId = questionnaireId;
            Status = status;
            FromDate = fromDate;
            ToDate = toDate;
            Tenant = tenant;
            Translation = translation;
            IncludeMeta = includeMeta;
        }

        public long? JobId { get; set; }
        public DataExportFormat ExportFormat { get; set; }
        public QuestionnaireIdentity QuestionnaireId { get; set; }
        public InterviewStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public TenantInfo Tenant { get; set; }
        
        public Guid? Translation { get; set; }

        public bool? IncludeMeta { get; set; }

        public string NaturalId => $"{Tenant}${InterviewStatusString()}${ExportFormat}${this.QuestionnaireId}" +
                                   $"${this.FromDate?.ToString(@"YYYYMMDD") ?? "EMPTY FROM DATE"}" +
                                   $"${this.ToDate?.ToString(@"YYYYMMDD") ?? "EMPTY TO DATE"}" +
                                   $"${this.Translation?.ToString("N") ?? "No translation"}" +
                                   ( this.IncludeMeta == false ? "no-meta" : "" );

        private string InterviewStatusString() => Status?.ToString() ?? "All";

        public override string ToString()
        {
            return
                $"{Tenant.Id} - {QuestionnaireId} {Status?.ToString() ?? "AllStatus"} {FromDate}-{ToDate}";
        }
    }
}
