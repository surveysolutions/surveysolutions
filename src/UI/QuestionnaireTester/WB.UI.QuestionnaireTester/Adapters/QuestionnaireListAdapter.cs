using System.Collections.Generic;
using System.Threading;
using Android.Content;
using Android.Views;
using Android.Widget;
using Main.Core.Utility;
using RestSharp;
using WB.UI.Shared.Android.Adapters;
using WB.UI.Shared.Android.RestUtils;

namespace WB.UI.QuestionnaireTester.Adapters
{
    public class QuestionnaireListAdapter:SmartAdapter<string>
    {
        private readonly IRestUrils webExecutor;
        private readonly Context context;
        public QuestionnaireListAdapter(Context context)
            : base()
        {
            this.webExecutor = new AndroidRestUrils("http://192.168.173.1/designer");
            items = this.webExecutor.ExcecuteRestRequestAsync<List<string>>(
                "Api/Tester/GetAllTemplates", new CancellationToken(), null,
                new HttpBasicAuthenticator("admin", "qwerty"));
            this.context = context;
        }

        protected override View BuildViewItem(string dataItem, int position)
        {
            LayoutInflater layoutInflater =
                (LayoutInflater) this.context.GetSystemService(Context.LayoutInflaterService);

            View view = layoutInflater.Inflate(Resource.Layout.template_list_item, null);
            var tvTitle =
                view.FindViewById<TextView>(Resource.Id.tvTitle);

            tvTitle.Text = dataItem;

          /*  var tvTryMe =
              view.FindViewById<TextView>(Resource.Id.tvTryMe);
            Animation anim = new AlphaAnimation(0.0f, 1.0f);
            anim.Duration= 50; //You can manage the time of the blink with this parameter
            anim.StartOffset =20;
            anim.RepeatMode= RepeatMode.Reverse;
            anim.RepeatCount = Animation.Infinite;
            tvTryMe.StartAnimation(anim);*/

            return view;
        }
    }
}