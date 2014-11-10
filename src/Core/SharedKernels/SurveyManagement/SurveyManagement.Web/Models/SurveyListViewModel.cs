using System.Collections.Generic;
using Main.Core.Entities;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class SurveyListViewModel : IGridRequest<SurveyRequestModel>
    {
        public PagerData Pager { get; set; }

        public SurveyRequestModel Request { get; set; }

        public IEnumerable<OrderRequestItem> SortOrder { get; set; }
    }
}