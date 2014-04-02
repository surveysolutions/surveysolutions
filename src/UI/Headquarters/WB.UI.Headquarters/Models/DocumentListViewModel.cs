using System.Collections.Generic;
using Main.Core.Entities;

namespace WB.UI.Headquarters.Models
{
    public class DocumentListViewModel : IGridRequest<DocumentRequestModel>
    {
        public PagerData Pager { get; set; }

        public DocumentRequestModel Request { get; set; }

        public IEnumerable<OrderRequestItem> SortOrder { get; set; }
    }
}