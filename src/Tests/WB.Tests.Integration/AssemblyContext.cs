using System;
using Machine.Specifications;
using Moq;
using Ncqrs;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Tests.Integration
{
    public class AssemblyContext : IAssemblyContext
    {
        public void OnAssemblyStart()
        {
            Setup.MockedServiceLocator();
        }

        public void OnAssemblyComplete() {}
    }
}
