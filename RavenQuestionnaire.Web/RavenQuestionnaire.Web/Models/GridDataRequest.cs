using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Web.Models
{
    public class GridDataRequest
    {
        public List<OrderRequestItem> SortOrder { get; set; }
        public PagerData Pager { get; set; }
    }

    public class PagerData
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
