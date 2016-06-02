﻿using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Questionnaire
{
    public class QuestionnaireBrowseView
    {
        private string _order = string.Empty;

        public QuestionnaireBrowseView(
            int page, int? pageSize, int totalCount, IEnumerable<QuestionnaireBrowseItem> items, string order)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = items;
            this.Order = order;
        }

        public IEnumerable<QuestionnaireBrowseItem> Items { get; private set; }

        public string Order
        {
            get
            {
                return this._order;
            }

            set
            {
                this._order = value;
            }
        }

        public int Page { get; private set; }

        public int? PageSize { get; private set; }

        public int TotalCount { get; private set; }
    }
}