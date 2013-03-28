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
        public DashboardSurveyItem(Guid publicKey, string surveyTitle, IEnumerable<DashboardQuestionnaireItem> items)
        {
            PublicKey = publicKey;
            SurveyTitle = surveyTitle;
            allItems = items.ToList();
        }
        public DashboardSurveyItem(Guid publicKey, string surveyTitle)
        {
            PublicKey = publicKey;
            SurveyTitle = surveyTitle;
            allItems = new List<DashboardQuestionnaireItem>();
        }
        public Guid PublicKey { get; private set; }
        public string SurveyTitle { get; private set; }
      /*  public IList<DashboardQuestionnaireItem> VisibleItems
        {
            get { return allItems.Where(i => i.Visible).ToList(); }
        }*/

        public IEnumerable<DashboardQuestionnaireItem> ActiveItems {
            get { return allItems.Where(i=>IsVisible(i.Status)); }
        }

        public DashboardQuestionnaireItem GetItem(Guid key)
        {
            return allItems.FirstOrDefault(i => i.PublicKey == key);
        }

        public void AddItem(DashboardQuestionnaireItem item)
        {
            if (GetItem(item.PublicKey) != null)
                Remove(item.PublicKey);
            allItems.Add(item);
        }

        public bool TryToChangeQuestionnaireState(Guid key, SurveyStatus status)
        {
            var questionnaire = allItems.FirstOrDefault(i => i.PublicKey == key);
            if (questionnaire != null)
            {
                questionnaire.SetStatus(status);
                return true;
            }
            return false;
        }



        public bool Remove(Guid key)
        {
            var currentItem = allItems.FirstOrDefault(i => i.PublicKey == key);
            if (currentItem != null)
                return allItems.Remove(currentItem);
            return false;
        }

        protected bool IsVisible(SurveyStatus status)
        {
            return status == SurveyStatus.Initial || status == SurveyStatus.Redo || status == SurveyStatus.Complete ||
                   status == SurveyStatus.Error;


        }

        private IList<DashboardQuestionnaireItem> allItems;
    }
}
