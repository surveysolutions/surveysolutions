using System.Collections.Generic;
using Main.Core.Entities;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class DesignerQuestionnairesListViewModel : IGridRequest<DesignerQuestionnairesRequestModel>
    {
        public PagerData Pager { get; set; }
        public DesignerQuestionnairesRequestModel Request { get; set; }
        public IEnumerable<OrderRequestItem> SortOrder { get; set; }
    }
}