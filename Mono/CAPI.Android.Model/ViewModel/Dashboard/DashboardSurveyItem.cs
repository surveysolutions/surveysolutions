// -----------------------------------------------------------------------
// <copyright file="DashboardSurveyItem.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;

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
            allItems = items;
        }
        public DashboardSurveyItem(Guid publicKey, string surveyTitle)
        {
            PublicKey = publicKey;
            SurveyTitle = surveyTitle;
            allItems = new List<DashboardQuestionnaireItem>();
        }
        public Guid PublicKey { get; private set; }
        public string SurveyTitle { get; private set; }
        public IList<DashboardQuestionnaireItem> VisibleItems
        {
            get { return allItems.Where(i => i.Visible).ToList(); }
        }

        public IList<DashboardQuestionnaireItem> AllItems {
            get { return allItems; }
        }

        public void AddItem(DashboardQuestionnaireItem item)
        {
            RemoveItem(item.PublicKey);
            allItems.Add(item);
        }

        public void RemoveItem(Guid key)
        {
            var currentItem = allItems.FirstOrDefault(i => i.PublicKey == key);
            if (currentItem != null)
                allItems.Remove(currentItem);
        }


        private IList<DashboardQuestionnaireItem> allItems;
    }
}
