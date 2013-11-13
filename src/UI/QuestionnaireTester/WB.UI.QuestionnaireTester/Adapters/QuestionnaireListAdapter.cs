using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
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
        private readonly Activity activity;
        private CancellationToken cancellationToken;
        private readonly ProgressDialog progressDialog;

        public QuestionnaireListAdapter(Activity activity)
            : base()
        {
            this.items = new List<string>();

            this.activity = activity;
            this.progressDialog = new ProgressDialog(activity);

            this.progressDialog.SetTitle("Loading");
            this.progressDialog.SetMessage("Uploading templates from Designer");
            this.progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
            this.progressDialog.SetCancelable(false);

            var tokenSource2 = new CancellationTokenSource();
            this.cancellationToken = tokenSource2.Token;
            progressDialog.Show();
            Task.Factory.StartNew(UploadQuestionnairesFromDesigner, this.cancellationToken);
        }

        protected void UploadQuestionnairesFromDesigner()
        {
            var webExecutor = new AndroidRestUrils("https://192.168.173.1/designer");
            items = webExecutor.ExcecuteRestRequestAsync<List<string>>(
                "Api/Tester/GetAllTemplates", cancellationToken, null,
                new HttpBasicAuthenticator("admin", "qwerty"), "GET");
            activity.RunOnUiThread(() =>
            {
                this.NotifyDataSetChanged();
                progressDialog.Hide();
            });
        }

        protected override View BuildViewItem(string dataItem, int position)
        {
            LayoutInflater layoutInflater =
                (LayoutInflater) this.activity.GetSystemService(Context.LayoutInflaterService);

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