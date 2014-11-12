using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using WB.Core.GenericSubdomains.Logging;

namespace WB.Core.BoundedContexts.Supervisor.Tests
{
    public class AssemblyContext : IAssemblyContext
    {
        public void OnAssemblyStart()
        {
            var serviceLocator = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object;

            ServiceLocator.SetLocatorProvider(() => serviceLocator);

            NcqrsEnvironment.SetGetter<ILogger>(Mock.Of<ILogger>);
            NcqrsEnvironment.SetGetter<IUniqueIdentifierGenerator>(Mock.Of<IUniqueIdentifierGenerator>);
            NcqrsEnvironment.SetGetter<IClock>(Mock.Of<IClock>);
        }

        public void OnAssemblyComplete() {}
    }
}
