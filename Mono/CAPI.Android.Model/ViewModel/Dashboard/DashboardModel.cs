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

        public void ReinitSurveys(IEnumerable<DashboardSurveyItem> updatedSurveyList)
        {
            foreach (DashboardSurveyItem dashboardSurveyItem in updatedSurveyList)
            {
                var  existingSurvey = GetSurvey(dashboardSurveyItem.PublicKey);

                foreach (DashboardQuestionnaireItem questionnaireItem in dashboardSurveyItem.ActiveItems)
                {
                    var existingQuestionnaire =
                        existingSurvey.ActiveItems.FirstOrDefault(q => q.PublicKey == questionnaireItem.PublicKey);
                    if (existingQuestionnaire != null)
                        existingQuestionnaire.SetStatus(questionnaireItem.Status);
                }
            }
        }
        /*
        private DashboardSurveyItem HandleNotExistingSurvey(DashboardSurveyItem dashboardSurveyItem)
        {
            var survey = new DashboardSurveyItem(dashboardSurveyItem.PublicKey, dashboardSurveyItem.SurveyTitle);
            Surveys.Add(survey);
            return survey;
        }

        */
        protected DashboardSurveyItem GetSurvey(Guid id)
        {
            return Surveys.FirstOrDefault(q => q.PublicKey == id);
        }
    }
}