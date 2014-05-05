namespace Web.Supervisor.Models
{
    using System.Collections.Generic;

    using Main.Core.Entities;

    public class StatusListViewModel : IGridRequest<StatusRequestModel>
    {
        public PagerData Pager { get; set; }

        public StatusRequestModel Request { get; set; }

        public IEnumerable<OrderRequestItem> SortOrder { get; set; }
    }
}