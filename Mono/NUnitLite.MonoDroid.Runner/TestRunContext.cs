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
    /// Contains context based information about the current test run.
    /// This context is used by the runner to have a central place where to find
    /// (non-serializable) test information for the various activities that make up the 
    /// test runner.
    /// </summary>
    public class TestRunContext
    {
        private static TestRunContext _current;
        private static object _lockHandle = new object();

        private List<TestRunInfo> _testResults;

        /// <summary>
        /// Initializes a new instance of <see cref="TestRunContext"/>
        /// </summary>
        private TestRunContext()
        {
            _testResults = new List<TestRunInfo>();
        }

        /// <summary>
        /// Gets the current test run context
        /// </summary>
        public static TestRunContext Current
        {
            get
            {
                if(_current == null)
                {
                    lock(_lockHandle)
                    {
                        if(_current == null)
                        {
                            _current = new TestRunContext();
                        }
                    }
                }

                return _current;
            }
        }

        /// <summary>
        /// Gets the test results for the current test run
        /// </summary>
        public List<TestRunInfo> TestResults
        {
            get { return _testResults; }
        }
    }
}