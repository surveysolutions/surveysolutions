using System;
using System.Linq.Expressions;
using AndroidMocks.Implementation;

namespace AndroidMocks
{
	public class DynamicMock<T>
	{
		public T Instance
		{
			get { return (T) _mock.GetTransparentProxy(); }
		}

		private readonly ICallHandler _callHandler;

		private readonly Mock<T> _mock;

		public DynamicMock()
		{
			_callHandler = new SimpleCallHandler();
			
			_mock = new Mock<T>(_callHandler);
		}

		private string GetMethodName(Expression expression)
		{
			var methodProp = expression.GetType().GetProperty("Method");
			var methodObj = methodProp.GetValue(expression, null);
			var methodNameProp = methodObj.GetType().GetProperty("Name");
			var actionName = (string)methodNameProp.GetValue(methodObj, null);

			return actionName;
		}

		#region Stub 

		public void Stub(string methodName, object returnValue = null)
		{
			_callHandler.Stub(methodName, returnValue);
		}

		public void Stub(Expression<Action<T>> action)
		{
			var actionName = GetMethodName(action.Body);

			Stub(actionName);
		}

		public void Stub<U>(Expression<Func<T,U>> action, U returnValue)
		{
			var actionName = GetMethodName(action.Body);

			Stub(actionName, returnValue);
		}

		#endregion

		#region StubAndThrow

		public void StubAndThrow<TException>(Expression<Action<T>> action) where TException : Exception, new()
		{
			var actionName = GetMethodName(action.Body);

			_callHandler.StubAndThrow<TException>(actionName);
		}

		public void StubAndThrow<TException,U>(Expression<Func<T,U>> action) where TException : Exception, new()
		{
			var actionName = GetMethodName(action.Body);

			_callHandler.StubAndThrow<TException>(actionName);
		}

		#endregion

		#region Expect

		public void Expect(string methodName, object returnValue = null)
		{
			_callHandler.Expect(methodName, returnValue);
		}

		public void Expect(Expression<Action<T>> action)
		{
			var actionName = GetMethodName(action.Body);

			Expect(actionName);
		}

		public void Expect<U>(Expression<Func<T, U>> action, U returnValue)
		{
			var actionName = GetMethodName(action.Body);

			Expect(actionName, returnValue);
		}

		#endregion

		#region Verification

		public void AssertWasCalled(Expression<Action<T>> action, int times = 1)
		{
			var actionName = GetMethodName(action.Body);

			_callHandler.AssertWasCalled(actionName, times);
		}

		public void AssertWasCalled<U>(Expression<Func<T, U>> action, int times = 1)
		{
			var actionName = GetMethodName(action.Body);

			_callHandler.AssertWasCalled(actionName, times);
		}

		public void VerifyAllExpectations()
		{
			_callHandler.VerifyAllExpectations();
		}

		public void Reset()
		{
			_callHandler.Reset();
		}

		#endregion
	}
}