using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using NUnitLite.Runner;

namespace NUnitLite.MonoDroid
{
    /// <summary>
    /// Derive from this activity to create a standard test runner activity in your app.
    /// </summary>
    public abstract class TestRunnerActivity : ListActivity
    {
        private TestResultsListAdapter _testResultsAdapter;

        /// <summary>
        /// Handles the creation of the activity
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _testResultsAdapter = new TestResultsListAdapter(this);
            ListAdapter = _testResultsAdapter;
        }

        protected override void OnResume()
        {
            base.OnResume();

            var testAssemblies = GetAssembliesForTest();
            var testAssemblyEnumerator = testAssemblies.GetEnumerator();
            var testRunner = new TestRunner();

            // Clear the test result list
            TestRunContext.Current.TestResults.Clear();

            _testResultsAdapter.NotifyDataSetInvalidated();
            _testResultsAdapter.NotifyDataSetChanged();

            // Add a test listener for the test runner
            testRunner.AddListener(new UITestListener((TestResultsListAdapter)ListAdapter));

            // Start the test process in a background task
            Task.Factory.StartNew(() =>
            {
                while (testAssemblyEnumerator.MoveNext())
                {
                    try
                    {
                        var assembly = testAssemblyEnumerator.Current;
                        testRunner.Run(assembly);
                    }
                    catch (Exception ex)
                    {
                        ShowErrorDialog(ex);
                    }
                }
            });
        }

        /// <summary>
        /// Handles list item click
        /// </summary>
        /// <param name="l"></param>
        /// <param name="v"></param>
        /// <param name="position"></param>
        /// <param name="id"></param>
        protected override void OnListItemClick(ListView l, View v, int position, long id)
        {
            var testRunItem = TestRunContext.Current.TestResults[position];

            Intent intent = new Intent(this,GetDetailsActivityType);
            intent.PutExtra("TestCaseName",testRunItem.TestCaseName);

            StartActivity(intent);
        }

        /// <summary>
        /// Retrieves a list of assemblies that contain test cases to execute using the test runner activity.
        /// </summary>
        /// <returns>Returns the list of assemblies to test</returns>
        protected abstract IEnumerable<TestAssemblyInfo> GetAssembliesForTest();

        /// <summary>
        /// Gets the type of activity to use for displaying test details
        /// </summary>
        protected abstract Type GetDetailsActivityType { get; }

        /// <summary>
        /// Displays an error dialog in case a test run fails
        /// </summary>
        /// <param name="exception"></param>
        private void ShowErrorDialog(Exception exception)
        {
            RunOnUiThread(() =>
            {
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetTitle("Failed to execute unit-test suite");
                builder.SetMessage(exception.ToString());

                var dialog = builder.Create();

                dialog.Show();
            });
        }
    }
}