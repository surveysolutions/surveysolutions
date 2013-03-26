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

namespace NUnitLite.MonoDroid
{
    /// <summary>
    /// Test listener that handles tests results on the user interface.
    /// </summary>
    public class UITestListener : ITestListener
    {
        private Handler _threadHandler;
        private TestResultsListAdapter _listAdapter;

        /// <summary>
        /// Initializes a new instance <see cref="UITestListener"/>
        /// </summary>
        /// <param name="listAdapter"></param>
        public UITestListener(TestResultsListAdapter listAdapter)
        {
            // Create a new thread handler for the main looper.
            // This handler is used to post code from the background thread
            // back to the UI thread.
            _threadHandler = new Handler(Application.Context.MainLooper);

            _listAdapter = listAdapter;
        }

        /// <summary>
        /// Handles when a test is started
        /// </summary>
        /// <param name="test"></param>
        public void TestStarted(ITest test)
        {
            _threadHandler.Post(() =>
            {
                TestRunContext.Current.TestResults.Add(new TestRunInfo()
                {
                    Description = test.Name,
                    TestCaseName = test.FullName,
                    Running = true,
                    Passed = false,
                    IsTestSuite = test is TestSuite
                });

                _listAdapter.NotifyDataSetInvalidated();
                _listAdapter.NotifyDataSetChanged();
            });
        }

        /// <summary>
        /// Handles the outcome of a test case
        /// </summary>
        /// <param name="result">The result.</param>
        public void TestFinished(TestResult result)
        {
            _threadHandler.Post(() =>
            {
                var testRunItem = TestRunContext.Current.TestResults
                    .FirstOrDefault(item => item.TestCaseName == result.Test.FullName);

                if (testRunItem != null)
                {
                    testRunItem.Passed = result.IsSuccess;
                    testRunItem.Running = false;
                    testRunItem.TestResult = result;
                }

                _listAdapter.NotifyDataSetInvalidated();
                _listAdapter.NotifyDataSetChanged();
            });
        }
    }
}