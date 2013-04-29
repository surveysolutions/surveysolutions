using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;

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

        public void ReinitSurveys(IEnumerable<DashboardSurveyItem> surveys)
        {
            foreach (var questionnaire in surveys.SelectMany(s=>s.ActiveItems).ToList())
            {
                var current = GetQuestionanire(questionnaire.PublicKey);
                if (current == null)
                {
                    var survey = GetSurvey(questionnaire.SurveyKey);
                    if (survey == null)
                        continue;
                    survey.ActiveItems.Add(questionnaire);
                    continue;
                }
                if (current.Status.PublicId != questionnaire.Status.PublicId)
                    current.SetStatus(questionnaire.Status);
                
            }
           /* foreach (DashboardSurveyItem dashboardSurveyItem in Surveys)
            {
                if(dashboardSurveyItem.ActiveItems.Count==0)
                    da
            }*/
        }

        protected DashboardQuestionnaireItem GetQuestionanire(Guid id)
        {
            return Surveys.SelectMany(s => s.ActiveItems).FirstOrDefault(q => q.PublicKey == id);
        }
        protected DashboardSurveyItem GetSurvey(Guid id)
        {
            return Surveys.FirstOrDefault(q => q.PublicKey == id);
        }
    }
}