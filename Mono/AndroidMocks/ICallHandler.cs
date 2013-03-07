using System;

namespace AndroidMocks
{
	public interface ICallHandler
	{
		object Call(string methodName, object[] args);

		void Expect(string methodName, object returnValue);

		void Stub(string methodName, object returnValue);

		void StubAndThrow<TException>(string methodName) where TException : Exception, new();

		void VerifyAllExpectations();
		
		void AssertWasCalled(string actionName, int times);

		void Reset();
	}
}