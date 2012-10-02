using System;
using System.Reflection;
using System.Collections;
using NUnit.Framework;

namespace NUnitLite
{
	public class TestCase : ITest
	{
		#region Instance Variables

		private object _fixture;
		private MethodInfo _method;

		private MethodInfo _setup;
		private MethodInfo _teardown;

		private IDictionary _properties;

		#endregion

		#region Constructors

		public TestCase(MethodInfo method)
		{
			RunState = RunState.Runnable;
			Initialize(method);
		}

		private void Initialize(MethodInfo method)
		{
			Name = method.Name;
			_method = method;
			FullName = method.ReflectedType.FullName + "." + Name;

			if (!HasValidSignature(method))
			{
				RunState = RunState.NotRunnable;
				IgnoreReason = "Test methods must have signature void MethodName()";
			}
			else
			{
				var ignore = (IgnoreAttribute) Reflect.GetAttribute(this._method, typeof (IgnoreAttribute));
				if (ignore != null)
				{
					RunState = RunState.Ignored;
					IgnoreReason = ignore.Reason;
				}
			}

			foreach (MethodInfo m in method.ReflectedType.GetMethods())
			{
				if (Reflect.HasAttribute(m, typeof (SetUpAttribute)))
					this._setup = m;

				if (Reflect.HasAttribute(m, typeof (TearDownAttribute)))
					this._teardown = m;
			}
		}

		#endregion

		#region Properties

		public string Name { get; private set; }

		public string FullName { get; private set; }

		public RunState RunState { get; private set; }

		public string IgnoreReason { get; private set; }

		public IDictionary Properties
		{
			get
			{
				if (_properties == null)
				{
					_properties = new Hashtable();

					var attrs = this._method.GetCustomAttributes(typeof (PropertyAttribute), true);
					foreach (PropertyAttribute attr in attrs)
						foreach (DictionaryEntry entry in attr.Properties)
							this.Properties[entry.Key] = entry.Value;
				}

				return _properties;
			}
		}

		public int TestCaseCount
		{
			get { return 1; }
		}

		#endregion

		#region Public Methods

		public static bool IsTestMethod(MethodInfo method)
		{
			return Reflect.HasAttribute(method, typeof (TestAttribute));
		}

		public TestResult Run(ITestListener listener, object objectFixture)
		{
			_fixture = objectFixture;

			listener.TestStarted(this);

			var result = new TestResult(this);

			Run(result, listener);

			listener.TestFinished(result);

			return result;
		}

		#endregion

		#region Private Methods

		private void SetUp()
		{
			if (_setup == null) return;

			Assert.That(HasValidSetUpTearDownSignature(_setup), "Invalid SetUp method: must return void and have no arguments");
			InvokeMethod(_setup);
		}

		private void TearDown()
		{
			if (_teardown == null) return;

			Assert.That(HasValidSetUpTearDownSignature(_teardown),
			            "Invalid TearDown method: must return void and have no arguments");
			InvokeMethod(_teardown);
		}

		private void Run(TestResult result, ITestListener listener)
		{
			var ignore = (IgnoreAttribute) Reflect.GetAttribute(_method, typeof (IgnoreAttribute));
			if (this.RunState == RunState.NotRunnable)
				result.Failure(this.IgnoreReason);
			else if (ignore != null)
				result.NotRun(ignore.Reason);
			else
			{
				try
				{
					RunBare();
					result.Success();
				}
				catch (NUnitLiteException nex)
				{
					result.RecordException(nex.InnerException);
				}
#if !NETCF_1_0
				catch (System.Threading.ThreadAbortException)
				{
					throw;
				}
#endif
				catch (Exception ex)
				{
					result.RecordException(ex);
				}
			}
		}

		private void RunTest()
		{
			try
			{
				InvokeMethod(_method);
				ProcessNoException(_method);
			}
			catch (NUnitLiteException ex)
			{
				ProcessException(_method, ex.InnerException);
			}
		}

		private void InvokeMethod(MethodInfo method, params object[] args)
		{
			Reflect.InvokeMethod(method, _fixture, args);
		}

		private static bool HasValidSignature(MethodInfo method)
		{
			return method != null
			       && method.ReturnType == typeof (void)
			       && method.GetParameters().Length == 0;
		}

		private void RunBare()
		{
			SetUp();
			try
			{
				RunTest();
			}
			finally
			{
				TearDown();
			}
		}

		private static bool HasValidSetUpTearDownSignature(MethodInfo method)
		{
			return method.ReturnType == typeof (void)
			       && method.GetParameters().Length == 0;
		}

		private static void ProcessNoException(MethodInfo method)
		{
			var exceptionAttribute =
				(ExpectedExceptionAttribute) Reflect.GetAttribute(method, typeof (ExpectedExceptionAttribute));

			if (exceptionAttribute != null)
				Assert.Fail("Expected Exception of type <{0}>, but none was thrown", exceptionAttribute.ExceptionType);
		}

		private void ProcessException(MethodInfo method, Exception caughtException)
		{
			var exceptionAttribute =
				(ExpectedExceptionAttribute) Reflect.GetAttribute(method, typeof (ExpectedExceptionAttribute));

			if (exceptionAttribute == null)
				throw new NUnitLiteException("", caughtException);

			var expectedType = exceptionAttribute.ExceptionType;
			if (expectedType != null && expectedType != caughtException.GetType())
				Assert.Fail("Expected Exception of type <{0}>, but was <{1}>", exceptionAttribute.ExceptionType,
				            caughtException.GetType());

			var handler = GetExceptionHandler(method.ReflectedType, exceptionAttribute.Handler);

			if (handler != null)
				InvokeMethod(handler, caughtException);
		}

		private MethodInfo GetExceptionHandler(Type type, string handlerName)
		{
			if (handlerName == null && Reflect.HasInterface(type, typeof (IExpectException)))
				handlerName = "HandleException";

			if (handlerName == null)
				return null;

			var handler = Reflect.GetMethod(type, handlerName,
			                                       BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
			                                       BindingFlags.Static,
			                                       new Type[] {typeof (Exception)});

			if (handler == null)
				Assert.Fail("The specified exception handler {0} was not found", handlerName);

			return handler;
		}

		#endregion
	}
}
