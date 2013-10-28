using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;

namespace WB.Core.BoundedContexts.Designer.Tests
{
    public class AssemblyContext : IAssemblyContext
    {
        public void OnAssemblyStart()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        public void OnAssemblyComplete() {}
    }
}
