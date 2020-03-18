using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.UI.Headquarters.Models.Api
{
    public class TeamsAndStatusesReportResponse : DataTableResponse<TeamsAndStatusesReportLine>
    {
        public TeamsAndStatusesReportLine TotalRow { get; set; }
    }
}