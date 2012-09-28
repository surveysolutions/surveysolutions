namespace FluentAssertions.Frameworks
{
	public class NUnitLiteTestFramework : LateBoundTestFramework
	{
		protected override string AssemblyName
		{
			get { return "NUnitLite.dll"; }
		}

		protected override string ExceptionFullName
		{
			get { return "NUnit.Framework.AssertionException"; }
		}
	}
}