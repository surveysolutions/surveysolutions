namespace AndroidMocks.Implementation
{
	public class SimpleMethodCall : IMethodCall
	{
		private readonly object _returnValue;

		public SimpleMethodCall(object returnValue)
		{
			_returnValue = returnValue;
		}

		public object Call(object[] args)
		{
			return _returnValue;
		}
	}
}