using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;

namespace NUnitLite.MonoDroid
{
    /// <summary>
    /// List adapter used to display test results
    /// </summary>
    public class TestResultsListAdapter : BaseAdapter
    {
        private Context _context;

        /// <summary>
        /// Initializes a new instance of <see cref="TestResultsListAdapter"/>
        /// </summary>
        /// <param name="context"></param>
        public TestResultsListAdapter(Context context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the number of items in the list
        /// </summary>
        public override int Count
        {
            get { return TestRunContext.Current.TestResults.Count; }
        }

        /// <summary>
        /// Gets an item from the list
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override Java.Lang.Object GetItem(int position)
        {
            return TestRunContext.Current.TestResults.ElementAt(position);
        }

        /// <summary>
        /// Gets the ID of an item in the list
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override long GetItemId(int position)
        {
            return position;
        }

        /// <summary>
        /// Gets the number of view types supported by this adapter
        /// </summary>
        public override int ViewTypeCount
        {
            get { return 2; }
        }

        /// <summary>
        /// Gets the view type for an item in the list
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override int GetItemViewType(int position)
        {
            var result = TestRunContext.Current.TestResults[position];

            if(result.IsTestSuite)
            {
                return 0;
            }

            return 1;
        }

        /// <summary>
        /// Gets the view for an item
        /// </summary>
        /// <param name="position"></param>
        /// <param name="convertView"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;
            var itemContent = TestRunContext.Current.TestResults[position];

            if (view == null)
            {
                view = InflateItemView(itemContent);
            }

            var tag = (TestResultViewHolder) view.Tag;

            // Fill the view with the correct settings
            tag.DescriptionTextView.Text = itemContent.Description;
            tag.IndicatorView.SetBackgroundColor(itemContent.Running ? Color.Gray : itemContent.Passed ? Color.Green : Color.Red);

            return view;
        }

        /// <summary>
        /// Gets whether all items are enabled
        /// </summary>
        /// <returns></returns>
        public override bool AreAllItemsEnabled()
        {
            return false;
        }

        /// <summary>
        /// Gets whether a specific item is enabled
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override bool IsEnabled(int position)
        {
            return !TestRunContext.Current.TestResults[position].IsTestSuite;
        }

        private View InflateItemView(TestRunInfo itemContent)
        {
            LinearLayout layout = new LinearLayout(_context);
            layout.Orientation = Orientation.Horizontal;
            
            View indicatorView = new View(_context);
            TextView descriptionView = new TextView(_context);

            descriptionView.Ellipsize = Android.Text.TextUtils.TruncateAt.Start;
            descriptionView.Gravity = GravityFlags.CenterVertical;
            
            if(itemContent.IsTestSuite)
            {
                layout.SetBackgroundColor(Color.Argb(255,50,50,50));
                
                indicatorView.LayoutParameters = new LinearLayout.LayoutParams(18, 18)
                {
                    LeftMargin = 14,
                    RightMargin = 14,
                    TopMargin = 14,
                    BottomMargin = 14,
                    Gravity = GravityFlags.CenterVertical
                };

                descriptionView.LayoutParameters = new LinearLayout.LayoutParams(
                    LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.FillParent)
                {
                    BottomMargin = 14,
                    TopMargin = 14,
                    RightMargin = 14,
                    Height = 48,
                    Gravity = GravityFlags.CenterVertical
                };
            }
            else
            {
                indicatorView.LayoutParameters = new LinearLayout.LayoutParams(18, LinearLayout.LayoutParams.FillParent)
                {
                    RightMargin = 20
                };

                descriptionView.LayoutParameters = new LinearLayout.LayoutParams(
                    LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.FillParent)
                {
                    BottomMargin = 20,
                    TopMargin = 20,
                    RightMargin = 20,
                    Gravity = GravityFlags.CenterVertical
                };
            }

            layout.AddView(indicatorView);
            layout.AddView(descriptionView);

            layout.Tag = new TestResultViewHolder(indicatorView,descriptionView);

            return layout;
        }
    }
}