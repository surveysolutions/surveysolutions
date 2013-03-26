using System;
using System.Reflection;

namespace AndroidMocks.Implementation
{
	public class SimpleThrowExceptionCall<TException> : IThrowExceptionCall where TException : Exception, new()
	{
		private readonly TException _exceptionInstance;

		public SimpleThrowExceptionCall()
		{
			_exceptionInstance = new TException();
		}

		public void Throw()
		{
			throw _exceptionInstance;
		}

		public object Call(object[] args)
		{
			throw _exceptionInstance;
		}
	}
}