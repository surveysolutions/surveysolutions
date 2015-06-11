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
            QuestionnaireDocument questionnaireDocument, 
            int pageIndex, 
            int totalCount, int pageSize)
        {
            Id = id;
            Title = title;
            ChangeHistory = changeHistory;
            PageIndex = pageIndex;
            TotalCount = totalCount;
            PageSize = pageSize;
            QuestionnaireDocument = questionnaireDocument;
        }

        public Guid Id { get;private set; }
        public string Title { get; private set; }
        public List<QuestionnaireChangeHistoricalRecord> ChangeHistory { get; private set; }
        public QuestionnaireDocument QuestionnaireDocument { get; private set; }
        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
    }
}