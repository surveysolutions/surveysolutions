using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core;
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
            ProgressBar pb=new ProgressBar(this);
            
           /* TextView tv=new TextView(this);
            var img = this.Resources.GetDrawable(Android.Resource.Drawable.SpinnerDropDownBackground);
            //img.SetBounds(0, 0, 45, 45);
            tv.SetCompoundDrawablesWithIntrinsicBounds(null, null, img, null);*/
            this.AddContentView(pb, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent));
            restore.BeginInvoke(Guid.Parse(Intent.GetStringExtra("publicKey")), Callback, restore);
            // Create your application here
        }
        private void Callback(IAsyncResult asyncResult)
        {
            Action<Guid> asyncAction = (Action<Guid>)asyncResult.AsyncState;
            asyncAction.EndInvoke(asyncResult);
        }
        protected void Restore(Guid publicKey)
        {
            try
            {
                var model = CapiApplication.LoadView<QuestionnaireScreenInput, CompleteQuestionnaireView>(
                    new QuestionnaireScreenInput(publicKey));
                
                Intent intent = new Intent(this, typeof(DetailsActivity));
                intent.PutExtra("publicKey", publicKey.ToString());
                StartActivity(intent);
            }
            catch
            {
                
            }
        }
    }
}