using System;
using System.Reflection;

namespace NUnitLite
{
	public class TestAssemblyInfo
	{
		public Assembly AssemblyToLoad { get; private set; }

		public Type[] TypesToTest { get; private set; }

		public TestAssemblyInfo(Assembly assemblyToLoad, params Type[] typesToTest)
		{
			AssemblyToLoad = assemblyToLoad;
			TypesToTest = typesToTest;
		}
	}
}