using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils.Implementation.Services;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.BoundedContexts.Capi.Tests
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

            var substitutionService = new SubstitutionService();

            serviceLocatorMock
                .Setup(locator => locator.GetInstance<ISubstitutionService>())
                .Returns(substitutionService);

            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

            NcqrsEnvironment.SetGetter<ILogger>(Mock.Of<ILogger>);
            NcqrsEnvironment.SetGetter<IUniqueIdentifierGenerator>(Mock.Of<IUniqueIdentifierGenerator>);
            NcqrsEnvironment.SetGetter<IClock>(Mock.Of<IClock>);
        }

        public void OnAssemblyComplete() {}
    }
}
