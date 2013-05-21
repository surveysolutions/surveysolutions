// -----------------------------------------------------------------------
// <copyright file="DashboardSurveyItem.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Entities.SubEntities;

namespace CAPI.Android.Core.Model.ViewModel.Dashboard
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DashboardSurveyItem 
    {
        public DashboardSurveyItem(Guid publicKey, string surveyTitle, IEnumerable<DashboardQuestionnaireItem> items)
        {
            PublicKey = publicKey;
            SurveyTitle = surveyTitle;
            cacheddItems = items.ToList();
        }
        public DashboardSurveyItem(Guid publicKey, string surveyTitle)
        {
            PublicKey = publicKey;
            SurveyTitle = surveyTitle;
        }
        public Guid PublicKey { get; private set; }
        public string SurveyTitle { get; private set; }

        public IList<DashboardQuestionnaireItem> ActiveItems {
            get { return cacheddItems; }
        }


        private IList<DashboardQuestionnaireItem> cacheddItems = new List<DashboardQuestionnaireItem>();

    }
}
