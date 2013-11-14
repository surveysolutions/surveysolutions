using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Main.Core.Utility;
using RestSharp;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Shared.Android.Adapters;
using WB.UI.Shared.Android.RestUtils;

namespace WB.UI.QuestionnaireTester.Adapters
{
    public class QuestionnaireListAdapter : SmartAdapter<QuestionnaireListItem>
    {
        private Activity activity;
        private CancellationToken cancellationToken;
        private ProgressBar progressDialog;

        public QuestionnaireListAdapter(Activity activity)
            : base()
        {
            this.items = new List<QuestionnaireListItem>();

            this.activity = activity;

            this.AddLoader();
            
            var tokenSource2 = new CancellationTokenSource();
            this.cancellationToken = tokenSource2.Token;
            Task.Factory.StartNew(UploadQuestionnairesFromDesigner, this.cancellationToken);
        }

        private void AddLoader()
        {
            this.progressDialog = new ProgressBar(activity);

            activity.AddContentView(this.progressDialog,
                new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent));

            Display display = activity.WindowManager.DefaultDisplay;
            Point size = new Point();
            display.GetSize(size);

            this.progressDialog.SetX(size.X/2);
            this.progressDialog.SetY(size.Y/2);
        }

        protected void UploadQuestionnairesFromDesigner()
        {
            items =
                CapiTesterApplication.DesignerServices.GetQuestionnaireListForCurrentUser(cancellationToken).Items;
            activity.RunOnUiThread(() =>
            {
                if (items == null)
                {
                    CapiTesterApplication.Membership.LogOff();
                }
                this.NotifyDataSetChanged();
                progressDialog.Visibility = ViewStates.Gone;
                
            });
        }

        protected override View BuildViewItem(QuestionnaireListItem dataItem, int position)
        {
            LayoutInflater layoutInflater =
                (LayoutInflater) this.activity.GetSystemService(Context.LayoutInflaterService);

            View view = layoutInflater.Inflate(Resource.Layout.template_list_item, null);
            var tvTitle =
                view.FindViewById<TextView>(Resource.Id.tvTitle);

            tvTitle.Text = dataItem.Title;

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