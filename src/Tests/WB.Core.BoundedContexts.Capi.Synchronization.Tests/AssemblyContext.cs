using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using WB.Core.GenericSubdomains.Logging;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests
{
    public class AssemblyContext : IAssemblyContext
    {
        public void OnAssemblyStart()
        {
            SetupServiceLocatorIfNeeded();
        }

        internal static void SetupServiceLocatorIfNeeded()
        {
            if (ServiceLocator.IsLocationProviderSet)
                return;

            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

            NcqrsEnvironment.SetGetter<ILogger>(Mock.Of<ILogger>);
            NcqrsEnvironment.SetGetter<IUniqueIdentifierGenerator>(Mock.Of<IUniqueIdentifierGenerator>);
            NcqrsEnvironment.SetGetter<IClock>(Mock.Of<IClock>);
        }

        public void OnAssemblyComplete() {}
    }
}
