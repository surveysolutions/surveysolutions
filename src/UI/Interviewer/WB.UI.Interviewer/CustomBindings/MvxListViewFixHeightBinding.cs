using Android.Views;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Views;
using WB.UI.Shared.Enumerator.CustomBindings;

namespace WB.UI.Interviewer.CustomBindings
{
    public class MvxListViewFixHeightBinding : BaseBinding<MvxListView, object>
    {
        public MvxListViewFixHeightBinding(MvxListView listView) : base(listView) { }

        protected override void SetValueToView(MvxListView listView, object value)
        {
            IMvxAdapter listAdapter = listView.Adapter;
            if (listAdapter == null) 
                return;

            ViewGroup.LayoutParams par = listView.LayoutParameters;
            if (par == null)
                return;

            int numberOfItems = listAdapter.Count;

            // Get total height of all items.
            int totalItemsHeight = 0;
            for (int itemPos = 0; itemPos < numberOfItems; itemPos++) 
            {
                View item = listAdapter.GetView(itemPos, null, listView);
                item.Measure(0, 0);
                totalItemsHeight += item.MeasuredHeight;
            }

            // Get total height of all item dividers.
            int totalDividersHeight = listView.DividerHeight * (numberOfItems - 1);

            // Set list height.
            par.Height = totalItemsHeight + totalDividersHeight;
            listView.LayoutParameters = par;
            listView.RequestLayout();
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }
    }
}