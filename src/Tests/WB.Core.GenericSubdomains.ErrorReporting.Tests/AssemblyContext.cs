using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;

namespace WB.Core.GenericSubdomains.ErrorReporting.Tests
{
    public class AssemblyContext : IAssemblyContext
    {
        public void OnAssemblyStart()
        {
            var serviceLocator = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object;

            ServiceLocator.SetLocatorProvider(() => serviceLocator);
        }

        public void OnAssemblyComplete() {}
    }
}
