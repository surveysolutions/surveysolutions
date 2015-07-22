using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class QuantityBySupervisorsReportModel : IGridRequest<QuantityBySupervisorsReportInputModel>
    {
        public PagerData Pager { get; set; }

        public QuantityBySupervisorsReportInputModel Request { get; set; }

        public IEnumerable<OrderRequestItem> SortOrder { get; set; }
    }
}