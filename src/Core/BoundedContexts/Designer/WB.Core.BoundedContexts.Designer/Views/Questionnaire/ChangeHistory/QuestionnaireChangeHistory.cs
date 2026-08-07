using System;
using System.Collections.Generic;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    public class QuestionnaireChangeHistory
    {
        public QuestionnaireChangeHistory(
            Guid id, 
            string title,
            List<QuestionnaireChangeHistoricalRecord> changeHistory, 
            int pageIndex, 
            int totalCount, 
            int pageSize,
            string? search = null)
        {
            Id = id;
            Title = title;
            ChangeHistory = changeHistory;
            PageIndex = pageIndex;
            TotalCount = totalCount;
            PageSize = pageSize;
            Search = search;
        }

        public Guid Id { get;private set; }
        public string Title { get; private set; }
        public List<QuestionnaireChangeHistoricalRecord> ChangeHistory { get; private set; }
        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public bool ReadonlyMode { get; set; }
        public string? Search { get; private set; }
    }
}