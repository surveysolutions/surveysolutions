using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;

namespace NUnitLite.MonoDroid
{
    /// <summary>
    /// Displays the details of a run
    /// </summary>
    public abstract class TestRunDetailsActivity: Activity
    {
        /// <summary>
        /// Handles the creation of the activity
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            string testCaseName = Intent.GetStringExtra("TestCaseName");
            var testRunInfo = TestRunContext.Current.TestResults.First(item => item.TestCaseName == testCaseName);

            SetContentView(CreateView(testRunInfo));
        }

        private View CreateView(TestRunInfo testRunDetails)
        {
            LinearLayout layout = new LinearLayout(this);
            layout.Orientation = Orientation.Vertical;

            TextView titleTextView = new TextView(this);
            titleTextView.LayoutParameters = new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.FillParent, 48);

            titleTextView.SetBackgroundColor(Color.Argb(255,50,50,50));
            titleTextView.SetPadding(20,0,20,0);

            titleTextView.Gravity = GravityFlags.CenterVertical;
            titleTextView.Text = testRunDetails.Description;
            titleTextView.Ellipsize = TextUtils.TruncateAt.Start;

            TextView descriptionTextView = new TextView(this);
            descriptionTextView.LayoutParameters = new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.WrapContent)
            {
                LeftMargin = 40,
                RightMargin = 40
            };

            descriptionTextView.Text = testRunDetails.TestResult.Message + 
                "\r\n\r\n" + testRunDetails.TestResult.StackTrace;

            ScrollView scrollView = new ScrollView(this);
            scrollView.LayoutParameters = new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.FillParent, LinearLayout.LayoutParams.FillParent);

            scrollView.AddView(descriptionTextView);

            layout.AddView(titleTextView);
            layout.AddView(scrollView);

            return layout;
        }
    }
}