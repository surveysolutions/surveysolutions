using System;
using System.Reflection;
using System.Runtime.Loader;
using MvvmCross.Base;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Integration
{
    public class AppDomainContext : IDisposable
    {
        private AppDomainContext(AssemblyLoadContext assemblyLoadContext)
        {
            AssemblyLoadContext = assemblyLoadContext;
        }

        public static AppDomainContext Create()
        {
            AssemblyLoadContext assemblyLoadContext = new TestAssemblyLoadContext();
            return new AppDomainContext(assemblyLoadContext);
        }

        public AppDomain Domain => AppDomain.CurrentDomain;

        public AssemblyLoadContext AssemblyLoadContext { get; }

        public void Dispose()
        {
            AssemblyLoadContext.Unload();
        }
    }

    class TestAssemblyLoadContext : AssemblyLoadContext
    {
        public TestAssemblyLoadContext() : base(isCollectible: true)
        {
        }

        protected override Assembly Load(AssemblyName name)
        {
            return null;
        }
    }
}
