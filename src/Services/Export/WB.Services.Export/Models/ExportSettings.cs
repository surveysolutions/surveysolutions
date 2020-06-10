﻿using System;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Models
{
    public class ExportSettings
    {
        public ExportSettings(DataExportFormat exportFormat, 
            QuestionnaireId questionnaireId, TenantInfo tenant,
            InterviewStatus? status = null, DateTime? fromDate = null, 
            DateTime? toDate = null, Guid? translation = null)
        {
            ExportFormat = exportFormat;
            QuestionnaireId = questionnaireId;
            Status = status;
            FromDate = fromDate;
            ToDate = toDate;
            Tenant = tenant;
            Translation = translation;
        }

        public DataExportFormat ExportFormat { get; set; }
        public QuestionnaireId QuestionnaireId { get; set; }
        public InterviewStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public TenantInfo Tenant { get; set; }
        
        public Guid? Translation { get; set; }

        public string NaturalId => $"{Tenant}${InterviewStatusString()}${ExportFormat}${this.QuestionnaireId}" +
                                   $"${this.FromDate?.ToString(@"YYYYMMDD") ?? "EMPTY FROM DATE"}" +
                                   $"${this.ToDate?.ToString(@"YYYYMMDD") ?? "EMPTY TO DATE"}" +
                                   $"${this.Translation?.ToString("N") ?? "No translation"}";

        private string InterviewStatusString() => Status?.ToString() ?? "All";

        public override string ToString()
        {
            return
                $"{Tenant.Id} - {QuestionnaireId} {Status?.ToString() ?? "AllStatus"} {FromDate}-{ToDate}";
        }
    }
}
