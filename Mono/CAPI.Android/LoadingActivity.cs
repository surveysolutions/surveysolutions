using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;

namespace CAPI.Android
{
    using global::Android.Content.PM;

    [Activity(Label = "Loading", NoHistory = true, 
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class LoadingActivity : Activity
    {
        private Action<Guid> restore; 
        protected override void OnCreate(Bundle bundle)
        {
            restore = Restore;
            base.OnCreate(bundle);
            var pb=new ProgressBar(this);
            this.AddContentView(pb, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent));
            restore.BeginInvoke(Guid.Parse(Intent.GetStringExtra("publicKey")), Callback, restore);
        }
        private void Callback(IAsyncResult asyncResult)
        {
            var asyncAction = (Action<Guid>)asyncResult.AsyncState;
            asyncAction.EndInvoke(asyncResult);
        }

        protected void Restore(Guid publicKey)
        {
            var questionnaire = CapiApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(
                new QuestionnaireScreenInput(publicKey));
            if (questionnaire == null)
            {
                this.RunOnUiThread(this.Finish);
                return;
            }
            var intent = new Intent(this, typeof (DetailsActivity));
            intent.PutExtra("publicKey", publicKey.ToString());
            StartActivity(intent);
        }
    }
}