namespace FluentAssertions.Frameworks
{
	public class NUnitLiteTestFramework : LateBoundTestFramework
	{
		protected override string AssemblyName
		{
			get { return "NUnitLite"; }
		}

		protected override string ExceptionFullName
		{
			get { return "NUnit.Framework.AssertionException"; }
		}
	}
}