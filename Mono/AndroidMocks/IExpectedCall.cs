namespace AndroidMocks
{
	public interface IExpectedCall : IMethodCall
	{
		bool WasExecuted { get; }

		int InvocationCount { get; }

		void Reset();
	}
}