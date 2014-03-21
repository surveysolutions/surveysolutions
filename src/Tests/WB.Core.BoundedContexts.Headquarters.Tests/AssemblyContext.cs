using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Authentication;

namespace WB.Core.BoundedContexts.Headquarters.Tests
{
    public class AssemblyContext : IAssemblyContext
    {
        public void OnAssemblyStart()
        {
            var serviceLocator = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            serviceLocator.Setup(x => x.GetInstance<CustomPasswordValidator>())
                .Returns(Create.CustomPasswordValidator());
            ServiceLocator.SetLocatorProvider(() => serviceLocator.Object);
        }

        public void OnAssemblyComplete() {}
    }
}
