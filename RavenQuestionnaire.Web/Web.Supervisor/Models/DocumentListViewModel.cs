namespace Web.Supervisor.Models
{
    using System.Collections.Generic;

    using Main.Core.Entities;

    public class DocumentListViewModel : IGridRequest<DocumentRequestModel>
    {
        #region Public Properties

        public PagerData Pager { get; set; }

        public DocumentRequestModel Request { get; set; }

        public IEnumerable<OrderRequestItem> SortOrder { get; set; }

        #endregion
    }
}