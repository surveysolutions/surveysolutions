using System;
using System.Collections.Generic;
using System.Linq;

namespace CAPI.Android.Core.Model.ViewModel.Dashboard
{
    public class DashboardSurveyItem
    {
        public DashboardSurveyItem(Guid questionnaireId, long maxVersion, string surveyTitle, IEnumerable<DashboardQuestionnaireItem> items)
        {
            this.QuestionnaireId = questionnaireId;
            this.SurveyTitle = surveyTitle;
            this.cachedItems = items.ToList();
            this.QuestionnaireMaxVersion = maxVersion;
        }

        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireMaxVersion { get; private set; }
        public string SurveyTitle { get; private set; }

        public IList<DashboardQuestionnaireItem> ActiveItems
        {
            get { return this.cachedItems; }
        }

        private IList<DashboardQuestionnaireItem> cachedItems = new List<DashboardQuestionnaireItem>();
    }
}
