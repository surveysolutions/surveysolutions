using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class AllQuestionnairesListViewModel
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<OrderRequestItem> SortOrder { get; set; }

        public string SeachFor { get; set; }
    }
}