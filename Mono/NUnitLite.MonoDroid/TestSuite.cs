using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace NUnitLite
{
	public class TestSuite : ITest
	{
		#region Instance Variables

		private readonly List<TestCase> _testCases = new List<TestCase>(10);

		private readonly Type _fixtureType;

		private object _fixtureObject;

		private RunState _runState;

		private string _ignoreReason;

		#endregion

		#region Constructors

		public TestSuite(Type type)
		{
			Properties = new Hashtable();
			_runState = RunState.Runnable;

			_fixtureType = type;

			object[] attrs = type.GetCustomAttributes(typeof (PropertyAttribute), true);
			foreach (PropertyAttribute attr in attrs)
				foreach (DictionaryEntry entry in attr.Properties)
					this.Properties[entry.Key] = entry.Value;

			var ignore = (IgnoreAttribute) Reflect.GetAttribute(type, typeof (IgnoreAttribute));
			if (ignore != null)
			{
				_runState = RunState.Ignored;
				_ignoreReason = ignore.Reason;
			}

			if (InvalidTestSuite(type)) return;

			foreach (MethodInfo method in type.GetMethods())
			{
				if (TestCase.IsTestMethod(method))
					_testCases.Add(new TestCase(method));
			}
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _fixtureType.Name; }
		}

		public string FullName 
		{
			get { return _fixtureType.FullName; }
		}

		public IDictionary Properties { get; private set; }

		public int TestCaseCount
		{
			get { return this._testCases.Sum(test => test.TestCaseCount); }
		}

		#endregion

		#region Public Methods

		public TestResult Run(ITestListener listener)
		{
			int count = 0, failures = 0, errors = 0;
			listener.TestStarted(this);
			TestResult result = new TestResult(this);

			switch (_runState)
			{
				case RunState.NotRunnable:
					result.Error(_ignoreReason);
					break;

				case RunState.Ignored:
					result.NotRun(_ignoreReason);
					break;

				case RunState.Runnable:
					_fixtureObject = CreateFextureObject();
					RunFixtureSetupIfNeeded();
					foreach (var test in _testCases)
					{
						++count;
						TestResult r = test.Run(listener, _fixtureObject);
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
					RunFixtureTeardownIfNeeded();
					if (count == 0)
						result.NotRun("Class has no tests");
					else if (errors > 0 || failures > 0)
						result.Failure("One or more component tests failed");
					else
						result.Success();
					break;
			}

			listener.TestFinished(result);
			return result;
		}

		#endregion

		#region Private Methods

		private void RunFixtureSetupIfNeeded()
		{
			var fixtureSetUp = _fixtureType.GetMethods()
				.FirstOrDefault(m => Reflect.HasAttribute(m, typeof (TestFixtureSetUpAttribute)));

			if (fixtureSetUp != null) 
				Reflect.InvokeMethod(fixtureSetUp, _fixtureObject);
		}

		private void RunFixtureTeardownIfNeeded()
		{
			var fixtureTearDown = _fixtureType.GetMethods()
				.FirstOrDefault(m => Reflect.HasAttribute(m, typeof(TestFixtureTearDownAttribute)));

			if (fixtureTearDown != null)
				Reflect.InvokeMethod(fixtureTearDown, _fixtureObject);
		}

		private object CreateFextureObject()
		{
			return Reflect.Construct(_fixtureType);
		}

		private bool InvalidTestSuite(Type type)
		{
			if (!Reflect.HasConstructor(type))
			{
				_runState = RunState.NotRunnable;
				_ignoreReason = string.Format("Class {0} has no default constructor", type.Name);
				return true;
			}

			return false;
		}

		#endregion
	}
}
