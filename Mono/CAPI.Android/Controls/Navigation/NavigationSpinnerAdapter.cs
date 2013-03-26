using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace CAPI.Android.Controls.Navigation
{
    public class NavigationSpinnerAdapter : BaseAdapter
    {
        private IList<NavigationItem> _spinnerItems;
        private LayoutInflater _layoutInflater;

        public NavigationSpinnerAdapter(Context context, IList<NavigationItem> items)
        {
            _spinnerItems = items;

           /* // Create java strings for this sample.
            // This saves a bit on JNI handles.
            _spinnerItems.Add("Dashboard");
            _spinnerItems.Add("Sync");
            if (CapiApplication.Membership.IsLoggedIn)
                _spinnerItems.Add("LogOff");*/

            // Retrieve the layout inflater from the provided context
            _layoutInflater = LayoutInflater.FromContext(context);
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
            var view = convertView;

            // Try to reuse views as much as possible.
            // It is alot faster than inflating new views all the time
            // and it saves quite a bit on memory usage aswell.
            if (view == null)
            {
                // inflate a new layout for the view.
                view = new TextView(parent.Context);
            }

            var textView = view as TextView;
            textView.Text = _spinnerItems[position].Title;
            textView.TextSize = 20;
            textView.Gravity = GravityFlags.Right;
            view.SetPadding(10,10,10,10);
            return view;
        }

        public override int Count
        {
            get { return _spinnerItems.Count; }
        }
    }
}