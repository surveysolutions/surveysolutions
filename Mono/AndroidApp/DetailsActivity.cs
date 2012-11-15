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

namespace AndroidApp
{
    [Activity(Label = "My Activity")]
    public class DetailsActivity : Activity
    {
        protected Guid QuestionnaireId
        {
            get { return Guid.Parse(Intent.GetStringExtra("questionnaireId")); }
        }

        protected override void OnCreate(Bundle bundle)
        {

            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Details);
            TextView tvId = new TextView(this);
            tvId.Text = this.QuestionnaireId.ToString();
            LinearLayout layout = FindViewById<LinearLayout>(Resource.Id.MyLayout);
            layout.AddView(tvId);
            // Create your application here
        }
    }
}