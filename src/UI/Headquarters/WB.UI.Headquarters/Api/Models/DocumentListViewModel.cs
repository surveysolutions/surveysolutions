using System.Collections.Generic;
using Main.Core.Entities;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Api.Models
{
    public class DocumentListViewModel
    {
        public PagerData Pager { get; set; }

        public DocumentRequestModel Request { get; set; }

        public IEnumerable<OrderRequestItem> SortOrder { get; set; }
    }
}