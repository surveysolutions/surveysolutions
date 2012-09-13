using System.Collections.Generic;
using Main.Core.Entities;

namespace Web.Supervisor.Models
{
    using System;

    using Main.Core.Entities;

    public class GridDataRequestModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public List<OrderRequestItem> SortOrder { get; set; }
        public PagerData Pager { get; set; }
        public Guid SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public Guid TemplateId { get; set; }
    }

    public class PagerData
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}