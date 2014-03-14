using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using NSubstitute;

namespace WB.UI.Headquarters.Tests
{
    public class AssemblyContext : IAssemblyContext
    {
        public void OnAssemblyStart()
        {
            ServiceLocator.SetLocatorProvider(() => Substitute.For<IServiceLocator>());
        }

        public void OnAssemblyComplete() {}
    }
}
