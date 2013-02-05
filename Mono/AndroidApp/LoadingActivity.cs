using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            ProgressBar pb=new ProgressBar(this);
            
           /* TextView tv=new TextView(this);
            var img = this.Resources.GetDrawable(Android.Resource.Drawable.SpinnerDropDownBackground);
            //img.SetBounds(0, 0, 45, 45);
            tv.SetCompoundDrawablesWithIntrinsicBounds(null, null, img, null);*/
            this.AddContentView(pb, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent));
            // Create your application here
        }
    }
}