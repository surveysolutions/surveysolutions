using Android.Graphics;
using Android.Views;
using AndroidX.Core.Content;
using MvvmCross;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Enumerator.Utils;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ViewGroupStatusBarColorByInterviewStatusBinding : BaseBinding<ViewGroup, GroupStatus>
    {
        private readonly ILogger logger;

        public ViewGroupStatusBarColorByInterviewStatusBinding(ViewGroup androidControl)
            : base(androidControl)
        {
            this.logger = Mvx.IoCProvider.Resolve<ILoggerProvider>().GetFor<ViewGroupStatusBarColorByInterviewStatusBinding>();
        }

        protected override void SetValueToView(ViewGroup target, GroupStatus value)
        {
            target.Post(() =>
            {
                switch (value)
                {
                    case GroupStatus.CompletedInvalid:
                    case GroupStatus.StartedInvalid:
                        SetBackgroundColor(target, Resource.Color.interviewStatusBarErrors);
                        break;

                    case GroupStatus.Completed:
                        SetBackgroundColor(target, Resource.Color.interviewStatusBarCompleted);
                        break;
                    case GroupStatus.Disabled:
                        SetBackgroundColor(target, Resource.Color.interviewStatusBarDisabled);
                        break;

                    case GroupStatus.Started:
                    case GroupStatus.NotStarted:
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

                var activity = target.GetActivity();
                if (activity == null)
                    return;

                Window window = activity.Window;
                if (window == null)
                    return;

                window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                window.SetStatusBarColor(color);
            }
            catch (ObjectDisposedException ex)
            {
                logger.Warn($"Cannot set status bar color - context or window is disposed. Color resource: {colorResourceId}", ex);
            }
            catch (InvalidOperationException ex)
            {
                logger.Warn($"Cannot set status bar color - invalid operation. Color resource: {colorResourceId}", ex);
            }
            catch (Java.Lang.IllegalStateException ex)
            {
                logger.Warn($"Cannot set status bar color - illegal state. Color resource: {colorResourceId}", ex);
            }
            catch (Exception ex)
            {
                logger.Error($"Unexpected error setting status bar color. Color resource: {colorResourceId}", ex);
            }
        }
    }
}
