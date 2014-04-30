﻿namespace Web.Supervisor.Models
{
    using System.Collections.Generic;

    using Main.Core.Entities;

    public class SurveyListViewModel : IGridRequest<SurveyRequestModel>
    {
        public PagerData Pager { get; set; }

        public SurveyRequestModel Request { get; set; }

        public IEnumerable<OrderRequestItem> SortOrder { get; set; }
    }
}