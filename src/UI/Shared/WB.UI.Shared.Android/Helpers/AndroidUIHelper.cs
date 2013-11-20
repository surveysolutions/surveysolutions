using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace WB.UI.Shared.Android.Helpers
{
    public static class AndroidUIHelper
    {
        public static void WaitForLongOperation(this Activity activity, Action<CancellationToken> operation, bool showProgressDialog = true)
        {

            var progressBar = showProgressDialog ? CreateProgressBar(activity) : null;
            var tokenSource2 = new CancellationTokenSource();
            var cancellationToken = tokenSource2.Token;
            
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
    }
}