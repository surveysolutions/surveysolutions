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
    /// Container to keep track of test runs
    /// </summary>
    public class TestRunInfo: Java.Lang.Object
    {
        /// <summary>
        /// Gets or sets whether the test run is passed
        /// </summary>
        public bool Passed { get; set; }

        /// <summary>
        /// Gets or sets whether the test case is running
        /// </summary>
        public bool Running { get; set; }

        /// <summary>
        /// Gets or sets whether this is a suite of tests
        /// </summary>
        public bool IsTestSuite { get; set; }

        /// <summary>
        /// Gets the description for the test run
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets the full name for the test case
        /// </summary>
        public string TestCaseName { get; set; }

        /// <summary>
        /// Gets the test result for this test run
        /// </summary>
        public NUnitLite.TestResult TestResult { get; set; }
    }
}