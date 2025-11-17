using Android.Graphics;
using Android.Views;
using AndroidX.Core.Content;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class DashboardStatusBarColorByDashboardGroupTypeBinding : BaseBinding<ViewGroup, DashboardGroupType>
    {
        public DashboardStatusBarColorByDashboardGroupTypeBinding(ViewGroup androidControl)
            : base(androidControl)
        {
        }

        protected override void SetValueToView(ViewGroup target, DashboardGroupType value)
        {
            // Defer window modifications until after the current layout pass
            // to prevent interrupting DrawerLayout measurement
            target.Post(() =>
            {
                switch (value)
                {
                    case DashboardGroupType.Assignments:
                        SetBackgroundColor(target, Resource.Color.assignmentsStatusBar);
                        break;
                    
                    case DashboardGroupType.InvalidInterviews:
                        SetBackgroundColor(target, Resource.Color.interviewStatusBarErrors);
                        break;

                    case DashboardGroupType.CompletedInterviews:
                        SetBackgroundColor(target, Resource.Color.interviewStatusBarCompleted);
                        break;
                    
                    case DashboardGroupType.None:
                        SetBackgroundColor(target, Resource.Color.interviewStatusBarDisabled);
                        break;

                    case DashboardGroupType.InProgressInterviews:
                    case DashboardGroupType.WebInterviews:
                    default:
                        SetBackgroundColor(target, Resource.Color.interviewStatusBarInProgress);
                        break;
                }
            });
        }

        private static void SetBackgroundColor(ViewGroup target, int colorResourceId)
        {
            try
            {
                var color = new Color(ContextCompat.GetColor(target.Context, colorResourceId));

                if (target.Context is Activity activity)
                {
                    Window window = activity.Window;
                    if (window != null)
                    {
                        window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                        window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                        window.SetStatusBarColor(color);
                    }
                }
            }
            catch
            {
                // Silently fail if context is no longer valid
            }
        }
    }
}

