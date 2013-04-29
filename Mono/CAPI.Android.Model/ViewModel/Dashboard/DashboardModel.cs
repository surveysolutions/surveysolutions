using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using Java.IO;

namespace CAPI.Android.Core.Model.ViewModel.Dashboard
{
    public class DashboardModel : MvxViewModel
    {
        public DashboardModel(Guid ownerKey)
        {
            Surveys = new List<DashboardSurveyItem>();
            OwnerKey = ownerKey;
        }
        public DashboardModel(IEnumerable<DashboardSurveyItem> surveys, Guid ownerKey)
        {
            Surveys = surveys.ToList();
            OwnerKey = ownerKey;
        }

        public Guid OwnerKey { get; private set; }

        public IList<DashboardSurveyItem> Surveys { get; private set; }

        public void ReinitSurveys(IList<DashboardSurveyItem> updatedSurveyList)
        {
            if (Surveys.Select(s => s.PublicKey).Intersect(updatedSurveyList.Select(u => u.PublicKey)).Count() !=
                Surveys.Count)
            {
                this.Surveys = updatedSurveyList.ToList();
                this.RaisePropertyChanged("Surveys");
                return;
            }
            foreach (DashboardSurveyItem dashboardSurveyItem in updatedSurveyList)
            {
                var existingSurvey = GetSurvey(dashboardSurveyItem.PublicKey);
                if (existingSurvey == null)
                {
                    throw new InvalidOperationException("survey is absent");
                }
                if (
                    existingSurvey.ActiveItems.Select(s => s.PublicKey)
                                  .Intersect(dashboardSurveyItem.ActiveItems.Select(u => u.PublicKey))
                                  .Count() != existingSurvey.ActiveItems.Count)
                {
                    existingSurvey.ReplaceItems(dashboardSurveyItem.ActiveItems);
                    break;
                }
                foreach (DashboardQuestionnaireItem questionnaireItem in dashboardSurveyItem.ActiveItems)
                {
                    var existingQuestionnaire =
                        existingSurvey.ActiveItems.FirstOrDefault(q => q.PublicKey == questionnaireItem.PublicKey);
                    if (existingQuestionnaire == null)
                        throw new InvalidOperationException("questionnaire is absent");
                    existingQuestionnaire.SetStatus(questionnaireItem.Status);
                }
            }
        }

        protected DashboardSurveyItem GetSurvey(Guid id)
        {
            return Surveys.FirstOrDefault(q => q.PublicKey == id);
        }
    }
}