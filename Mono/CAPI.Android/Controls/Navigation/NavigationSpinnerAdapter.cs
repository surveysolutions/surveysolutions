using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace CAPI.Android.Controls.Navigation
{
    public class NavigationSpinnerAdapter : BaseAdapter
    {
        private IList<NavigationItem> _spinnerItems;

        public NavigationSpinnerAdapter(Context context, IList<NavigationItem> items)
        {
            _spinnerItems = items;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return _spinnerItems[position].Title;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {

            if (convertView != null)
            {
                return convertView;
              
            }
            // inflate a new layout for the view.
            var view = new TextView(parent.Context);
            view.Text = _spinnerItems[position].Title;
            view.TextSize = 20;
            view.Gravity = GravityFlags.Right;
            view.SetPadding(10,10,10,10);
            return view;
        }

        public override int Count
        {
            get { return _spinnerItems.Count; }
        }
    }
}