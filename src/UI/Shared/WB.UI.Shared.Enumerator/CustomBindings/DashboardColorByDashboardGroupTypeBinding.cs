using Android.Graphics;
using Android.Views;
using AndroidX.Core.Content;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class DashboardColorByDashboardGroupTypeBinding : BaseBinding<ViewGroup, DashboardGroupType>
    {
        public DashboardColorByDashboardGroupTypeBinding(ViewGroup androidControl)
            : base(androidControl)
        {
        }

        protected override void SetValueToView(ViewGroup target, DashboardGroupType value)
        {
            switch (value)
            {
                case DashboardGroupType.Assignments:
                    SetBackgroundColor(target, Resource.Color.assignmentsStatusBar);
                    break;
                
                case DashboardGroupType.InvalidInterviews:
                    SetBackgroundColor(target, Resource.Color.interviewHeaderErrors);
                    break;

                case DashboardGroupType.CompletedInterviews:
                    SetBackgroundColor(target, Resource.Color.interviewHeaderCompleted);
                    break;
                case DashboardGroupType.Unknown:
                    SetBackgroundColor(target, Resource.Color.interviewHeaderDisabled);
                    break;

                case DashboardGroupType.InProgressInterviews:
                case DashboardGroupType.WebInterviews:
                default:
                    SetBackgroundColor(target, Resource.Color.interviewHeaderInProgress);
                    break;
            }
        }

        private static void SetBackgroundColor(ViewGroup target, int colorResourceId)
        {
            var color = new Color(ContextCompat.GetColor(target.Context, colorResourceId));
            target.SetBackgroundColor(color);
        }
    }
}
