// -----------------------------------------------------------------------
// <copyright file="DashboardSurveyItem.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace AndroidApp.ViewModel.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DashboardSurveyItem
    {
        public DashboardSurveyItem(Guid publicKey, string surveyTitle, IList<DashboardQuestionnaireItem> items)
        {
            PublicKey = publicKey;
            SurveyTitle = surveyTitle;
            Items = items;
        }

        public Guid PublicKey { get; private set; }
        public string SurveyTitle { get; private set; }
        public IList<DashboardQuestionnaireItem> Items { get; private set; }
    }
}
