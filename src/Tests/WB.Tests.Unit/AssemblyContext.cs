using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;

namespace WB.Tests.Unit
{
    public class AssemblyContext : IAssemblyContext
    {
        public void OnAssemblyStart()
        {
            SetupServiceLocator();
        }

        public void OnAssemblyComplete() {}

        public static void SetupServiceLocator()
        {
            if (ServiceLocator.IsLocationProviderSet)
                return;

            var serviceLocator = Stub<IServiceLocator>.WithNotEmptyValues;

            ServiceLocator.SetLocatorProvider(() => serviceLocator);
        }
    }
}
