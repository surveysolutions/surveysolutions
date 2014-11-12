using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils.Implementation.Services;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Tests.Unit.BoundedContexts.Designer
{
    public class AssemblyContext : IAssemblyContext
    {
        public void OnAssemblyStart()
        {
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };

            var substitutionService = new SubstitutionService();
            serviceLocatorMock
                .Setup(locator => locator.GetInstance<ISubstitutionService>())
                .Returns(substitutionService);

            serviceLocatorMock
                .Setup(locator => locator.GetInstance<IKeywordsProvider>())
                .Returns(new KeywordsProvider(substitutionService));

            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

            NcqrsEnvironment.SetGetter<ILogger>(Mock.Of<ILogger>);
            NcqrsEnvironment.SetGetter<IUniqueIdentifierGenerator>(Mock.Of<IUniqueIdentifierGenerator>);
            NcqrsEnvironment.SetGetter<IClock>(Mock.Of<IClock>);

        }

        public void OnAssemblyComplete() {}
    }
}
