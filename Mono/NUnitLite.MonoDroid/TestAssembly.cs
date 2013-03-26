using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NUnitLite
{
	public class TestAssembly : ITest
	{
		public string Name { get; private set; }

		public string FullName
		{
			get { return null; }
		}

		public RunState RunState 
		{ 
			get { return RunState.Runnable; } 
		}
		public string IgnoreReason
		{
			get { return null; }
		}

		public int TestCaseCount
		{
			get { return TestSuits.Sum(ts => ts.TestCaseCount); }
		}
		
		public IDictionary Properties { get; private set; }

		public IList<TestSuite> TestSuits { get; set; }

		public TestAssembly(string name)
		{
			this.Name = name;

			TestSuits = new List<TestSuite>();
		}

		public TestResult Run(ITestListener listener)
		{
			int count = 0, failures = 0, errors = 0;
			listener.TestStarted(this);
			TestResult result = new TestResult(this);

			foreach (var test in TestSuits)
			{
				++count;
				TestResult r = test.Run(listener);
				result.AddResult(r);
				switch (r.ResultState)
				{
					case ResultState.Error:
						++errors;
						break;
					case ResultState.Failure:
						++failures;
						break;
				}
			}

			if (count == 0)
				result.NotRun("Class has no tests");
			else if (errors > 0 || failures > 0)
				result.Failure("One or more component tests failed");
			else
				result.Success();

			listener.TestFinished(result);
			return result;
		}

		public void AddTest(TestSuite testSuite)
		{
			TestSuits.Add(testSuite);
		}
	}
}