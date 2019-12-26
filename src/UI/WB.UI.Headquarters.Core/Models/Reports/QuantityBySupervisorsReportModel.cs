using System;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;
using WB.UI.Headquarters.Models.Api;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class QuantityBySupervisorsReportModel : DataTableRequest
    {
        public DateTime From { get; set; }
        public Guid? QuestionnaireId { get; set; }
        public long? QuestionnaireVersion { get; set; }
        public string Period { get; set; }
        public int ColumnCount { get; set; }
        public PeriodiceReportType ReportType { get; set; }
        public int TimezoneOffsetMinutes { get; set; }
    }
}
