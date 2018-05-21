using System;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.UI.Headquarters.Models.Api;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class SurveyStatisticsInput : DataTableRequest
    {
        public string QuestionnaireId { get; set; }
        public string Question { get; set; }
        public Guid? TeamLeadId { get; set; }
        public bool DetailedView => this.Mode == ReportMode.WithInterviewers;

        public int? Min { get; set; }
        public int? Max { get; set; }
        public ExportFileType? exportType { get; set; }
        public string ConditionalQuestion { get; set; }
        public int[] Condition { get; set; }
        public bool Pivot { get; set; }

        public ReportMode Mode { get; set; }

        public enum ReportMode
        {
            TeamLeads, WithInterviewers
        }
    }
}
