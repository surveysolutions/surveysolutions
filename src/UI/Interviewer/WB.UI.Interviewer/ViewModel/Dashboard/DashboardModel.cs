using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;

namespace WB.UI.Interviewer.ViewModel.Dashboard
{
    public class DashboardModel : MvxViewModel
    {
        public DashboardModel(Guid ownerKey)
        {
            this.Surveys = new List<DashboardSurveyItem>();
            this.OwnerKey = ownerKey;
        }
        public DashboardModel(IEnumerable<DashboardSurveyItem> surveys, Guid ownerKey)
        {
            this.Surveys = surveys.ToList();
            this.OwnerKey = ownerKey;
        }

        public Guid OwnerKey { get; private set; }

        public IList<DashboardSurveyItem> Surveys { get; private set; }

    }
}