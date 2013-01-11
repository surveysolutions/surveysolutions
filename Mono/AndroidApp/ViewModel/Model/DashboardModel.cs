using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Main.Core.View.CompleteQuestionnaire;
using Cirrious.MvvmCross.ViewModels;

namespace AndroidApp.ViewModel.Model
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