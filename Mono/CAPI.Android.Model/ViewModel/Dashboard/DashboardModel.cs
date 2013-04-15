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
        public DashboardSurveyItem GetSurvey(Guid key)
        {
            return Surveys.FirstOrDefault(s => s.PublicKey == key);
        }
    }
}