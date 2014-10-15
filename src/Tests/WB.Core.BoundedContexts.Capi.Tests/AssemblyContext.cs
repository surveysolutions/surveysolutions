using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.SharedKernels.ExpressionProcessor.Implementation.Services;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

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

            serviceLocatorMock
                .Setup(locator => locator.GetInstance<IKeywordsProvider>())
                .Returns(new KeywordsProvider(substitutionService));

            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);
        }

        public void OnAssemblyComplete() {}
    }
}
