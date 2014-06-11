using System.Collections.Generic;
using Main.Core.Entities;

namespace WB.UI.Headquarters.Models
{
    public class DesignerQuestionnairesListViewModel : IGridRequest<DesignerQuestionnairesRequestModel>
    {
        public PagerData Pager { get; set; }
        public DesignerQuestionnairesRequestModel Request { get; set; }
        public IEnumerable<OrderRequestItem> SortOrder { get; set; }
    }
}