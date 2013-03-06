using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;

namespace CAPI.Android.Core.Model.ViewModel.Dashboard
{
    public class DashboardModel : MvxViewModel
    {
        public DashboardModel()
        {
            Surveys = new List<DashboardSurveyItem>();
        }
        public DashboardModel(IEnumerable<DashboardSurveyItem> surveys)
        {
            Surveys = surveys.ToList();
        }

        public IList<DashboardSurveyItem> Surveys { get; private set; }
    }
}