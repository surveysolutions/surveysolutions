using System.Collections.Generic;
using Main.Core.Entities;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class SummaryListViewModel : IGridRequest<SummaryRequestModel>
    {
        public PagerData Pager { get; set; }

        public SummaryRequestModel Request { get; set; }

        public IEnumerable<OrderRequestItem> SortOrder { get; set; }
    }
}