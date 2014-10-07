using System.Collections.Generic;
using Main.Core.Entities;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class AllQuestionnairesListViewModel : IGridRequest<AllQuestionnairesRequestModel>
    {
        public PagerData Pager { get; set; }
        public AllQuestionnairesRequestModel Request { get; set; }
        public IEnumerable<OrderRequestItem> SortOrder { get; set; }
    }
}