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
    public class DashboardSurveyItem :MvxViewModel
    {
        public DashboardSurveyItem(Guid publicKey, string surveyTitle, IEnumerable<DashboardQuestionnaireItem> items)
        {
            PublicKey = publicKey;
            SurveyTitle = surveyTitle;
            allItems = items.ToDictionary(k => k.PublicKey, v => v);
        }
        public DashboardSurveyItem(Guid publicKey, string surveyTitle)
        {
            PublicKey = publicKey;
            SurveyTitle = surveyTitle;
            allItems = new Dictionary<Guid, DashboardQuestionnaireItem>();
        }
        public Guid PublicKey { get; private set; }
        public string SurveyTitle { get; private set; }

        public IList<DashboardQuestionnaireItem> ActiveItems {
            get { return cacheddItems; }
        }

        private IList<DashboardQuestionnaireItem> cacheddItems = new List<DashboardQuestionnaireItem>();

        private void RecacheItems()
        {
            cacheddItems = allItems.Where(i => IsVisible(i.Value.Status)).Select(i => i.Value).ToList();
            this.RaisePropertyChanged("ActiveItems");
        }

        public DashboardQuestionnaireItem this[Guid key]
        {
            get
            {
                {
                    if (!allItems.ContainsKey(key))
                        return null;
                    return allItems[key];
                }
            }
        }

        public void AddItem(DashboardQuestionnaireItem item)
        {
            if (!allItems.ContainsKey(item.PublicKey))
            {
                allItems[item.PublicKey] = item;             
            }
            else
            {
                allItems[item.PublicKey].SetStatus(item.Status);
            }
            RecacheItems();
        }

        /* public bool TryToChangeQuestionnaireState(Guid key, SurveyStatus status)
        {
            var questionnaire = allItems.FirstOrDefault(i => i.PublicKey == key);
            if (questionnaire != null)
            {
                questionnaire.SetStatus(status);
                return true;
            }
            return false;
        }*/

        public bool Remove(Guid key)
        {
            if (!allItems.ContainsKey(key))
                return false;
            allItems.Remove(key);
            RecacheItems();
            return true;
        }

        protected bool IsVisible(SurveyStatus status)
        {
            return status == SurveyStatus.Initial || status == SurveyStatus.Redo || status == SurveyStatus.Complete ||
                   status == SurveyStatus.Error;
        }

        private IDictionary<Guid,DashboardQuestionnaireItem> allItems;
    }
}
