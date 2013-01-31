using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Commands;
namespace AndroidApp.Core.Model.ViewModel.Dashboard
{
    public class DashboardModel : MvxViewModel
    {
        public DashboardModel(IEnumerable<DashboardSurveyItem> surveys)
        {
            Surveys = surveys.ToList();
        }

        public IList<DashboardSurveyItem> Surveys { get; private set; }
    }
}