using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

namespace WB.UI.Shared.Android.Helpers
{
    public static class AndroidUIHelper
    {
        public static CancellationTokenSource WaitForLongOperation(this Activity activity, Action<CancellationToken> operation, bool showProgressDialog = true)
        {
            var progressBar = showProgressDialog ? CreateProgressBar(activity) : null;
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            
            Task.Factory.StartNew(() =>
            {
                operation(cancellationToken);

                if (showProgressDialog)
                {
                    activity.RunOnUiThread(() =>
                    {
                        progressBar.Visibility = ViewStates.Gone;
                    });
                }
            }, cancellationToken);

            return cancellationTokenSource;
        }

        private static ProgressBar CreateProgressBar(Activity activity)
        {
            var progressBar = new ProgressBar(activity);

            activity.AddContentView(progressBar,
                new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent));
            
            Display display = activity.WindowManager.DefaultDisplay;
            Point size = new Point();
            display.GetSize(size);

            progressBar.SetX(size.X / 2);
            progressBar.SetY(size.Y / 2);
            return progressBar;
        }

        public static void AttachCheckAndClearFocusForPanel(this ListView view, Activity activity)
        {
            view.ScrollStateChanged += (sender, args) =>
            {
                if (args.ScrollState == ScrollState.TouchScroll)
                {
                    var currentFocus = activity.CurrentFocus;
                    if (currentFocus != null)
                    {
                        currentFocus.ClearFocus();

                        InputMethodManager imm = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);
                        imm.HideSoftInputFromWindow(view.WindowToken, 0);
                    }
                }
            };
        }

    }
}