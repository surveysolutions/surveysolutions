using Microsoft.Practices.ServiceLocation;
using Moq;

namespace WB.Tests.Integration
{
    internal static class Setup
    {
        public static void MockedServiceLocator()
        {
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);
        }
    }
}