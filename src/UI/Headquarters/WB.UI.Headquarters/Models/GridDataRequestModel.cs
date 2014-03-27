using System;
using System.Collections.Generic;
using Main.Core.Entities;

namespace WB.UI.Headquarters.Models
{
    public class GridDataRequestModel
    {
        public List<OrderRequestItem> SortOrder { get; set; }

        public PagerData Pager { get; set; }

        public Guid? TemplateId { get; set; }

        public Guid? StatusId { get; set; }

        public Guid? InterviwerId { get; set; }
    }
}