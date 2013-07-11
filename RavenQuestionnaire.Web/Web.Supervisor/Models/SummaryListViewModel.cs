namespace Web.Supervisor.Models
{
    using System.Collections.Generic;

    using Main.Core.Entities;

    public class SummaryListViewModel : IGridRequest<SummaryRequestModel>
    {
        #region Public Properties

        public PagerData Pager { get; set; }

        public SummaryRequestModel Request { get; set; }

        public IEnumerable<OrderRequestItem> SortOrder { get; set; }

        #endregion
    }
}