using System.Reflection;
using System.Runtime.Loader;

namespace WB.UI.WebTester.Services.Implementation
{
    public class InterviewAssemblyLoadContext : AssemblyLoadContext
    {
        public InterviewAssemblyLoadContext(string binPath)
        {
            resolver = new AssemblyDependencyResolver(binPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }
    }
}
