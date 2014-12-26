using System;
using Machine.Specifications;
using Moq;
using Ncqrs;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Tests.Integration
{
    public class AssemblyContext : IAssemblyContext
    {
        public void OnAssemblyStart()
        {
            Setup.MockedServiceLocator();

            NcqrsEnvironment.SetDefault(Mock.Of<ILogger>());
            NcqrsEnvironment.SetDefault(Mock.Of<IUniqueIdentifierGenerator>(x => x.GenerateNewId() == Guid.NewGuid()));
            NcqrsEnvironment.SetGetter(() => Mock.Of<IUniqueIdentifierGenerator>(x => x.GenerateNewId() == Guid.NewGuid()));
            NcqrsEnvironment.SetGetter<ILogger>(Mock.Of<ILogger>);
            NcqrsEnvironment.SetGetter<IUniqueIdentifierGenerator>(Mock.Of<IUniqueIdentifierGenerator>);
            NcqrsEnvironment.SetGetter<IClock>(Mock.Of<IClock>);

            NcqrsEnvironment.Deconfigure();
        }

        public void OnAssemblyComplete() {}
    }
}
