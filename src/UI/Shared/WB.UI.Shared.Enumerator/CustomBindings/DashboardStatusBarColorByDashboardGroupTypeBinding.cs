using Android.Graphics;
using Android.Views;
using AndroidX.Core.Content;
using MvvmCross;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class DashboardStatusBarColorByDashboardGroupTypeBinding : BaseBinding<ViewGroup, DashboardGroupType>
    {
        private readonly ILogger logger;

        public DashboardStatusBarColorByDashboardGroupTypeBinding(ViewGroup androidControl)
            : base(androidControl)
        {
            this.logger = Mvx.IoCProvider.Resolve<ILoggerProvider>().GetFor<DashboardStatusBarColorByDashboardGroupTypeBinding>();
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

        private void SetBackgroundColor(ViewGroup target, int colorResourceId)
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
            catch (ObjectDisposedException ex)
            {
                logger.Warn($"Cannot set status bar color - context or window is disposed. Color resource: {colorResourceId}", ex);
            }
            catch (InvalidOperationException ex)
            {
                logger.Warn($"Cannot set status bar color - invalid operation. Color resource: {colorResourceId}", ex);
            }
            catch (Exception ex)
            {
                logger.Error($"Unexpected error setting status bar color. Color resource: {colorResourceId}", ex);
            }
        }
    }
}

