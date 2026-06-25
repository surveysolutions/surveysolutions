using Android.Graphics;
using Android.Views;
using AndroidX.Core.Content;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ViewGroupStatusBarColorByInterviewStatusBinding : BaseBinding<ViewGroup, GroupStatus>
    {
        public ViewGroupStatusBarColorByInterviewStatusBinding(ViewGroup androidControl)
            : base(androidControl)
        {
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

        private static void SetBackgroundColor(ViewGroup target, int colorResourceId)
        {
            try
            {
                var color = new Color(ContextCompat.GetColor(target.Context, colorResourceId));

                if (target.Context is not Activity activity)
                    return;

                Window window = activity.Window;
                if (window == null)
                    return;

                window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                window.SetStatusBarColor(color);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}
