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

            NcqrsEnvironment.SetDefault(Mock.Of<ILogger>());
            NcqrsEnvironment.SetGetter<ILogger>(Mock.Of<ILogger>);
            NcqrsEnvironment.SetGetter<IClock>(Mock.Of<IClock>);

            NcqrsEnvironment.Deconfigure();
        }

        public void OnAssemblyComplete() {}
    }
}
