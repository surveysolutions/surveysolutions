namespace NUnitLite
{
	public interface ITest
	{
		string Name { get; }
		string FullName { get; }

		System.Collections.IDictionary Properties { get; }

		//TestResult Run(ITestListener listener, object fixtureObject = null);
	}
}
