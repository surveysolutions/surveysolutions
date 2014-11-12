using Machine.Specifications;
using Moq;
using Ncqrs;
using WB.Core.GenericSubdomains.Logging;

namespace WB.Tests.Integration
{
    public class AssemblyContext : IAssemblyContext
    {
        public void OnAssemblyStart()
        {
            Setup.MockedServiceLocator();

            NcqrsEnvironment.SetGetter<ILogger>(Mock.Of<ILogger>);
            NcqrsEnvironment.SetGetter<IUniqueIdentifierGenerator>(Mock.Of<IUniqueIdentifierGenerator>);
            NcqrsEnvironment.SetGetter<IClock>(Mock.Of<IClock>);
        }

        public void OnAssemblyComplete() {}
    }
}
