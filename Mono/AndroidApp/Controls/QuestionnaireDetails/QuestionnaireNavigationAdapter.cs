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
using AndroidApp.ViewModel.QuestionnaireDetails;

namespace AndroidApp.Controls.QuestionnaireDetails
{
    public class QuestionnaireNavigationAdapter : BaseAdapter<QuestionnaireNavigationPanelItem>
    {
        private readonly IList<QuestionnaireNavigationPanelItem> items;
        private readonly Context context;

        public QuestionnaireNavigationAdapter(Context context, IEnumerable<QuestionnaireNavigationPanelItem> items)
            : base()
        {
            this.context = context;
            this.items = items.ToList();
            this.items.Add(new QuestionnaireNavigationPanelItem(Guid.Empty, "Complete", 0, 0));
        }


        #region Overrides of BaseAdapter

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;
            if (view == null)
            {
                LayoutInflater layoutInflater = (LayoutInflater) context.GetSystemService(Context.LayoutInflaterService);
                // no view to re-use, create new
                view = layoutInflater.Inflate(Resource.Layout.list_navigation_item, null);
            }
            var tvITem = view.FindViewById<TextView>(Resource.Id.tvITem);
            tvITem.Text = item.Title;
            var tvCount = view.FindViewById<TextView>(Resource.Id.tvCount);
            if (item.ScreenPublicKey != Guid.Empty)
            {
                tvCount.Text = string.Format("{0}/{1}", item.Answered, item.Total);
            }
            else
            {
                tvCount.Visibility = ViewStates.Gone;
            }
            view.SetTag(Resource.Id.ScreenId, item.ScreenPublicKey.ToString());
            return view;
        }

        public override int Count
        {
            get { return items.Count; }
        }

        #endregion

        #region Overrides of BaseAdapter<QuestionnaireNavigationPanelItem>

        public override QuestionnaireNavigationPanelItem this[int position]
        {
            get
            {
                return this.items[position];
            }
        }

        #endregion
    }
}