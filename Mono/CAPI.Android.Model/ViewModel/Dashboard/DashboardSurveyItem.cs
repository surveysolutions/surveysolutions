// -----------------------------------------------------------------------
// <copyright file="DashboardSurveyItem.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace CAPI.Android.Core.Model.ViewModel.Dashboard
{
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
        public DashboardSurveyItem(Guid publicKey, string surveyTitle)
        {
            PublicKey = publicKey;
            SurveyTitle = surveyTitle;
            Items = new List<DashboardQuestionnaireItem>();
        }
        public Guid PublicKey { get; private set; }
        public string SurveyTitle { get; private set; }
        public IList<DashboardQuestionnaireItem> Items { get; private set; }
    }
}
