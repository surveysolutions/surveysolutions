using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities;

namespace Web.Supervisor.Models
{
    public class GridDataRequestModel
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public List<OrderRequestItem> SortOrder { get; set; }
        public PagerData Pager { get; set; }
        public string SupervisorId { get; set; }
        public string SupervisorName { get; set; }
    }

    public class PagerData
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}