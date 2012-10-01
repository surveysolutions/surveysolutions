namespace AndroidMocks.Implementation
{
	public class SimpleExpectedMethodCall : IExpectedCall
	{
		private readonly object _returnValue;
		private int _invocationCounter = 0;

		public SimpleExpectedMethodCall(object returnValue)
		{
			_returnValue = returnValue;
		}

		public object Call(object[] args)
		{
			_invocationCounter++;
			return _returnValue;
		}

		public bool WasExecuted
		{
			get { return _invocationCounter > 0; }
		}

		public int InvocationCount
		{
			get { return _invocationCounter; }
		}

		public void Reset()
		{
			_invocationCounter = 0;
		}
	}
}