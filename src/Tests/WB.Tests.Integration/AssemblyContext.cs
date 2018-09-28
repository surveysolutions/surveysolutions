using System.Globalization;
using System.Threading;
using Moq;
using Ncqrs;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;

namespace WB.Tests.Integration
{
    [SetUpFixture]
    public class AssemblyContext 
    {
        [OneTimeSetUp]
        public void OnAssemblyStart()
        {
            SetUp.MockedServiceLocator();
        }

        public void OnAssemblyComplete() { }

        public static void SetupServiceLocator()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            if (ServiceLocator.IsLocationProviderSet)
                return;

            var serviceLocator = Stub<IServiceLocator>.WithNotEmptyValues;

            ServiceLocator.SetLocatorProvider(() => serviceLocator);

            SetUp.InstanceToMockedServiceLocator<ISubstitutionService>(new SubstitutionService());
            SetUp.InstanceToMockedServiceLocator<IKeywordsProvider>(new KeywordsProvider(new SubstitutionService()));
            SetUp.InstanceToMockedServiceLocator<IFileSystemAccessor>(new FileSystemIOAccessor());

            SetUp.InstanceToMockedServiceLocator<ILogger>(Mock.Of<ILogger>());
            SetUp.InstanceToMockedServiceLocator<IClock>(Mock.Of<IClock>());
        }

        internal static class Stub<T> where T : class
        {
            public static T WithNotEmptyValues => new Mock<T> { DefaultValue = DefaultValue.Mock }.Object;
        }
    }
}
