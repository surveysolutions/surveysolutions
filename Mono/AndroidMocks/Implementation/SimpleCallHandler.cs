using System;
using System.Collections.Generic;
using System.Linq;

namespace AndroidMocks.Implementation
{
	public class SimpleCallHandler : ICallHandler
	{
		private Dictionary<string, IMethodCall> _stubedCalls = new Dictionary<string, IMethodCall>();
		private Dictionary<string, IExpectedCall> _expectedCalls = new Dictionary<string, IExpectedCall>();
		private Dictionary<string, IThrowExceptionCall> _throwExceptionsCalls = new Dictionary<string, IThrowExceptionCall>();

		public SimpleCallHandler()
		{
			
		}

		private Dictionary<string, IMethodCall> _allCalls = new Dictionary<string, IMethodCall>();

		public object Call(string methodName, object[] args)
		{
			return _allCalls[methodName].Call(args);
		}

		public void Expect(string methodName, object returnValue)
		{
			var expectedCall = new SimpleExpectedMethodCall(returnValue);

			_expectedCalls[methodName] = expectedCall;
			_allCalls[methodName] = expectedCall;
		}

		public void Stub(string methodName, object returnValue)
		{
			var stubedCall = new SimpleMethodCall(returnValue);
			
			_stubedCalls[methodName] = stubedCall;
			_allCalls[methodName] = stubedCall;
		}

		public void StubAndThrow<TException>(string methodName) where TException : Exception, new()
		{
			var throwableCall = new SimpleThrowExceptionCall<TException>();

			_throwExceptionsCalls[methodName] = throwableCall;
			_allCalls[methodName] = throwableCall;
		}

		public void VerifyAllExpectations()
		{
			if (_expectedCalls.Any(p => !p.Value.WasExecuted))
				throw new ExpectationException();
		}

		public void AssertWasCalled(string actionName, int times)
		{
			if (_expectedCalls[actionName].InvocationCount != times)
				throw new ExpectationException();
		}

		public void Reset()
		{
			foreach (var expectedCall in _expectedCalls)
			{
				expectedCall.Value.Reset();
			}
		}
	}
}