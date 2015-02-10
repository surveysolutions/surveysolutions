using Android.App;
using Android.Views;
using Android.Widget;
using WB.Core.SharedKernels.DataCollection;

namespace WB.UI.QuestionnaireTester.Extensions
{
    public static class ActivityExtensions
    {
        public static void CreateActionBar(this Activity activity, IAnswerProgressIndicator answerProgressIndicator)
        {
            // Set up your ActionBar
            ActionBar actionBar = activity.ActionBar;
            actionBar.SetDisplayShowHomeEnabled(false);
            actionBar.SetDisplayShowTitleEnabled(false);
            actionBar.SetDisplayShowCustomEnabled(true);
            actionBar.SetDisplayUseLogoEnabled(true);
            actionBar.SetCustomView(Resource.Layout.ActionBar);

            var tvTitlte = (TextView) actionBar.CustomView.FindViewById(Resource.Id.tvTitlte);
            tvTitlte.Text = activity.Title;

            var imgProgress = (ImageView)actionBar.CustomView.FindViewById(Resource.Id.imgAnswerProgress);

            answerProgressIndicator.Setup(
                show: () => activity.RunOnUiThread(() => imgProgress.Visibility = ViewStates.Visible),
                hide: () => activity.RunOnUiThread(() => imgProgress.Visibility = ViewStates.Invisible));
        }

        
    }
}