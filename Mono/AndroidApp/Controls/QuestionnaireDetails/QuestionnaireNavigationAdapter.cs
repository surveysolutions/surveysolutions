using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.ViewModel.QuestionnaireDetails;

namespace AndroidApp.Controls.QuestionnaireDetails
{
    public class QuestionnaireNavigationAdapter : BaseAdapter<QuestionnaireScreenViewModel>
    {
        private readonly IList<QuestionnaireScreenViewModel> items;
        private readonly Context context;
        private readonly int selectedItem;
        public QuestionnaireNavigationAdapter(Context context, IEnumerable<QuestionnaireScreenViewModel> items, int selectedItem)
            : base()
        {
            this.context = context;
            this.selectedItem = selectedItem;
            this.items = items.ToList();
          //  this.items.Add(new QuestionnaireNavigationPanelItem(Guid.Empty, "Complete", 0, 0));
        }


        #region Overrides of BaseAdapter

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {

            View view = convertView;
            if (view == null)
            {
                LayoutInflater layoutInflater = (LayoutInflater) context.GetSystemService(Context.LayoutInflaterService);
                // no view to re-use, create new
                view = layoutInflater.Inflate(Resource.Layout.list_navigation_item, null);

                if (position == selectedItem)
                {
                    view.SetBackgroundColor(Color.LightBlue);
                }

                var tvITem = view.FindViewById<TextView>(Resource.Id.tvITem);

                var tvCount = view.FindViewById<TextView>(Resource.Id.tvCount);
                if (position < Count - 1)
                {
                    var item = items[position];
                    item.PropertyChanged += (sender,e)=>
                    {
                        if (e.PropertyName != "Answered" && e.PropertyName != "Total")
                            return;
                        var question = sender as QuestionnaireScreenViewModel;
                        if (question == null)
                            return;
                        UpdateCounter(tvCount, item);
                    }
                    ;
                    tvITem.Text = item.ScreenName;
                    UpdateCounter(tvCount, item);
                    view.SetTag(Resource.Id.ScreenId, item.ScreenId.ToString());
                }
                else
                {
                    tvITem.Text = "Complete";
                    tvCount.Visibility = ViewStates.Gone;
                }
            }
            return view;
        }
        protected void UpdateCounter(TextView tvCount, QuestionnaireScreenViewModel item)
        {
            tvCount.Text = string.Format("{0}/{1}", item.Answered, item.Total);
            if(item.Total==item.Answered)
                tvCount.SetBackgroundResource(Resource.Drawable.donecountershape);
            else
                tvCount.SetBackgroundResource(Resource.Drawable.CounterRoundShape);
        }


        public override int Count
        {
            get { return items.Count+1; }
        }

        #endregion

        #region Overrides of BaseAdapter<QuestionnaireNavigationPanelItem>

        public override QuestionnaireScreenViewModel this[int position]
        {
            get
            {
                return this.items[position];
            }
        }

        #endregion
    }
}