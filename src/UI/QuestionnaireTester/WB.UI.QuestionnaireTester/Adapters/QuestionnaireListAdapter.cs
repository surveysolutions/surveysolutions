using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Shared.Android.Helpers;

namespace WB.UI.QuestionnaireTester.Adapters
{
    public class QuestionnaireListAdapter : BaseAdapter<QuestionnaireListItem>
    {
        private readonly Activity activity;
        private IList<QuestionnaireListItem> unfilteredList;
        protected IList<QuestionnaireListItem> items;

        public QuestionnaireListAdapter(Activity activity)
        {
            items = new List<QuestionnaireListItem>();
            unfilteredList = new List<QuestionnaireListItem>();

            this.activity = activity;

            activity.WaitForLongOperation((ct)=>this.UploadQuestionnairesFromDesigner(ct));
        }

        protected async Task UploadQuestionnairesFromDesigner(CancellationToken cancellationToken)
        {
            if (CapiTesterApplication.DesignerMembership.IsLoggedIn)
            {
                try
                {
                    QuestionnaireListCommunicationPackage questionnaireListPackage =
                        await CapiTesterApplication.DesignerServices.GetQuestionnaireListForCurrentUser(
                            CapiTesterApplication.DesignerMembership.RemoteUser, cancellationToken);

                    unfilteredList = items = questionnaireListPackage.Items;
                }
                catch (Exception exc) 
                {
                    ShowLongToastInUIThread(exc.Message);
                    activity.RunOnUiThread(() =>
                    {
                        if (items == null)
                        {
                            CapiTesterApplication.DesignerMembership.LogOff();
                        }
                    });
                }

                activity.RunOnUiThread(NotifyDataSetChanged);
            }

            else
            {
                activity.RunOnUiThread(() =>
                {
                    if (items == null)
                    {
                        CapiTesterApplication.DesignerMembership.LogOff();
                    }
                });
            }
            
        }

        public void Update()
        {
            activity.WaitForLongOperation((ct) => UploadQuestionnairesFromDesigner(ct), false);
        }

        public void Query(string searchQuery)
        {
            if (!string.IsNullOrEmpty(searchQuery))
            {
                items =
                    unfilteredList.Where(
                        i =>
                            CultureInfo.InvariantCulture.CompareInfo.IndexOf(i.Title, searchQuery,
                                CompareOptions.IgnoreCase) >= 0).ToList();
            }
            else
            {
                items = unfilteredList;
            }
            NotifyDataSetChanged();
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            QuestionnaireListItem dataItem = this[position];
            View view = BuildViewItem(dataItem, position);
         
            return view;
        }

        protected View BuildViewItem(QuestionnaireListItem dataItem, int position)
        {
            var layoutInflater =
                (LayoutInflater) activity.GetSystemService(Context.LayoutInflaterService);

            var view = layoutInflater.Inflate(Resource.Layout.template_list_item, null) as LinearLayout;
            var tvTitle =
                view.FindViewById<TextView>(Resource.Id.tvTitle);

            tvTitle.Text = dataItem.Title;


            var tvArrow =
                view.FindViewById<TextView>(Resource.Id.tvArrow);
            Drawable img = activity.Resources.GetDrawable(Android.Resource.Drawable.IcMediaPlay);
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
            get { return items.Count; }
        }

        public override QuestionnaireListItem this[int position]
        {
            get { return items[position]; }
        }

        private void ShowLongToastInUIThread(string message)
        {
            this.activity.RunOnUiThread(() => Toast.MakeText(activity, message, ToastLength.Long).Show());
        }
    }
}