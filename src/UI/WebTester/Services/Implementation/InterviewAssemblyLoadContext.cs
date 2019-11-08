using System;
using System.Reflection;
using System.Runtime.Loader;

namespace WB.UI.WebTester.Services.Implementation
{
    public class InterviewAssemblyLoadContext : AssemblyLoadContext, IDisposable
    {
        private AssemblyDependencyResolver resolver;

        public InterviewAssemblyLoadContext(string binPath) : base(isCollectible: true)
        {
            resolver = new AssemblyDependencyResolver(binPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }

        public void Dispose()
        {
            this.Unload();
        }
    }
}
