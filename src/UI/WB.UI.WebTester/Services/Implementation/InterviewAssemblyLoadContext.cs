using System;
using System.Reflection;
using System.Runtime.Loader;

namespace WB.UI.WebTester.Services.Implementation
{
    public class InterviewAssemblyLoadContext : AssemblyLoadContext, IDisposable
    {
        public InterviewAssemblyLoadContext() : base(isCollectible: true)
        {
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            return null;
        }

        public void Dispose()
        {
            this.Unload();
        }
    }
}
