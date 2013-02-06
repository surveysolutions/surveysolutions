using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.Core;
using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails;

namespace AndroidApp
{
    [Activity(Label = "Loading", NoHistory = true)]
    public class LoadingActivity : MvxSimpleBindingFragmentActivity<CompleteQuestionnaireView>
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
            restore.BeginInvoke(ViewModel.PublicKey, Callback, restore);
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
                ViewModel = CapiApplication.LoadView<QuestionnaireScreenInput, CompleteQuestionnaireView>(
                    new QuestionnaireScreenInput(publicKey));
                ViewModel.Restore();
                
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