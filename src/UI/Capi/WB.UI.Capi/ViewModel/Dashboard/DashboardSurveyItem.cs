using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.UI.Capi.ViewModel.Dashboard
{
    public class DashboardSurveyItem
    {
        public DashboardSurveyItem(string id, string questionnaireId, long questionnaireVersion, string surveyTitle, IEnumerable<DashboardQuestionnaireItem> items, bool allowCensusMode)
        {
            this.AllowCensusMode = !string.IsNullOrEmpty(questionnaireId) && allowCensusMode;
            this.QuestionnaireId = string.IsNullOrEmpty(questionnaireId) ? Guid.Parse(id) : Guid.Parse(questionnaireId);
            this.SurveyTitle = surveyTitle;
            this.cachedItems = items.ToList();
            this.QuestionnaireVersion = questionnaireVersion;
        }

        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireVersion { get; private set; }
        public string SurveyTitle { get; private set; }
        public bool AllowCensusMode { get; private set; }

        public IList<DashboardQuestionnaireItem> ActiveItems
        {
            get { return this.cachedItems; }
        }

        private readonly IList<DashboardQuestionnaireItem> cachedItems = new List<DashboardQuestionnaireItem>();
    }
}
