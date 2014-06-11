using System.Collections.Generic;
using Main.Core.Entities;

namespace WB.UI.Headquarters.Models
{
    public class StatusListViewModel : IGridRequest<StatusRequestModel>
    {
        public PagerData Pager { get; set; }

        public StatusRequestModel Request { get; set; }

        public IEnumerable<OrderRequestItem> SortOrder { get; set; }
    }
}