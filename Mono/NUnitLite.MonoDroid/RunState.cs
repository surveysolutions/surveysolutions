namespace NUnitLite
{
	/// <summary>
	/// The RunState enum indicates whether a test
	/// has been or can be executed.
	/// </summary>
	public enum RunState
	{
		/// <summary>
		/// The test is not runnable
		/// </summary>
		NotRunnable,

		/// <summary>
		/// The test is runnable
		/// </summary>
		Runnable,

		/// <summary>
		/// The test can only be run explicitly
		/// </summary>
		//Explicit,

		/// <summary>
		/// The test has been skipped
		/// </summary>
		//Skipped,

		/// <summary>
		/// The test has been ignored
		/// </summary>
		Ignored

		/// <summary>
		/// The test has been executed
		/// </summary>
		//Executed
	}
}