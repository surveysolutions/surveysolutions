using System.Collections.Generic;
using System.Globalization;
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
    public class QuestionnaireListAdapter : BaseAdapter<QuestionnaireListItem>
    {
        private Activity activity;
        private CancellationToken cancellationToken;
        private ProgressBar progressDialog;
        private IList<QuestionnaireListItem> unfilteredList;
        protected IList<QuestionnaireListItem> items;

        public QuestionnaireListAdapter(Activity activity)
            : base()
        {
            this.items = new List<QuestionnaireListItem>();
            this.unfilteredList=new List<QuestionnaireListItem>();

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
            unfilteredList = items =
                CapiTesterApplication.DesignerServices.GetQuestionnaireListForCurrentUser(cancellationToken).Items;

           
            activity.RunOnUiThread(() =>
            {
                if (items == null)
                {
                    CapiTesterApplication.DesignerMembership.LogOff();
                }
                this.NotifyDataSetChanged();
                progressDialog.Visibility = ViewStates.Gone;
                
            });
        }

        public void Update()
        {
            Task.Factory.StartNew(UploadQuestionnairesFromDesigner, this.cancellationToken);
        }

        public void Query(string searchQuery)
        {
            if (!string.IsNullOrEmpty(searchQuery))
            {
                items = unfilteredList.Where(i => CultureInfo.InvariantCulture.CompareInfo.IndexOf(i.Title, searchQuery, CompareOptions.IgnoreCase) >= 0).ToList();
            }
            else
            {
                items = unfilteredList;
            }
            this.NotifyDataSetChanged();
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {

            var dataItem = this[position];
            var view = this.BuildViewItem(dataItem, position);
          /*  if (convertView == null)
            {
                convertView = this.CreateViewElement(dataItem, position);
            }
            else
            {
                var elementId = Convert.ToInt32(convertView.GetTag(Resource.Id.ElementId).ToString());
                if (elementId != position)
                {
                    convertView.TryClearBindingsIfPossible();

                    convertView = this.CreateViewElement(dataItem, position);
                }
            }*/

            return view;
        }
        protected View BuildViewItem(QuestionnaireListItem dataItem, int position)
        {
            LayoutInflater layoutInflater =
                (LayoutInflater) this.activity.GetSystemService(Context.LayoutInflaterService);

            var view = layoutInflater.Inflate(Resource.Layout.template_list_item, null) as LinearLayout;
            var tvTitle =
                view.FindViewById<TextView>(Resource.Id.tvTitle);

            tvTitle.Text = dataItem.Title;


            var tvArrow =
              view.FindViewById<TextView>(Resource.Id.tvArrow);
            var img = activity.Resources.GetDrawable(global::Android.Resource.Drawable.IcMediaPlay);
            tvArrow.SetCompoundDrawablesWithIntrinsicBounds(null, null, img, null);
            view.SetTag(Resource.Id.QuestionnaireId, dataItem.Id.ToString());
            return view;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int Count
        {
            get { return this.items.Count; }
        }

        public override QuestionnaireListItem this[int position]
        {
            get { return this.items[position]; }
        }
    }
}