using System;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Interviewer.Properties;
using WB.UI.Shared.Enumerator.CustomBindings;

namespace WB.UI.Interviewer.CustomBindings
{
    public class ViewInterviewStatusBinding : BaseBinding<View, DashboardInterviewCategories>
    {
        public ViewInterviewStatusBinding(View androidControl) : base(androidControl) { }

        public override MvxBindingMode DefaultMode { get { return MvxBindingMode.OneWay; } }

        protected override void SetValueToView(View control, DashboardInterviewCategories status)
        {
            switch (status)
            {
                case DashboardInterviewCategories.New:
                    SetBackground(control, Resource.Color.dashboard_new_interview_status);
                    break;
                case DashboardInterviewCategories.InProgress:
                    SetBackground(control, Resource.Color.dashboard_in_progress_tab);
                    break;
                case DashboardInterviewCategories.Complited:
                    SetBackground(control, Resource.Color.dashboard_complited_tab);
                    break;
                case DashboardInterviewCategories.Rejected:
                    SetBackground(control, Resource.Color.dashboard_rejected_tab);
                    break;
            }
        }

        private void SetBackground(View control, int colorId)
        {
            var color = control.Resources.GetColor(colorId);
            control.SetBackgroundColor(color);
        }
    }
}