using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.SharedKernels.ExpressionProcessor.Implementation.Services;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Core.BoundedContexts.Designer.Tests
{
    public class AssemblyContext : IAssemblyContext
    {
        public void OnAssemblyStart()
        {
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };

            serviceLocatorMock
                .Setup(locator => locator.GetInstance<ISubstitutionService>())
                .Returns(new SubstitutionService());

            serviceLocatorMock
                .Setup(locator => locator.GetInstance<IVariableNameValidator>())
                .Returns(new VariableNameValidator());

            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);
        }

        public void OnAssemblyComplete() {}
    }
}
