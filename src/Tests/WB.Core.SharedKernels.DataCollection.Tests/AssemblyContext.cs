using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.SharedKernels.DataCollection.Tests
{
    public class AssemblyContext : IAssemblyContext
    {
        public void OnAssemblyStart()
        {
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };

            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

            var fileSystemIoAccessor = new FileSystemIOAccessor();

            serviceLocatorMock
                .Setup(locator => locator.GetInstance<IFileSystemAccessor>())
                .Returns(fileSystemIoAccessor);

            NcqrsEnvironment.SetGetter<ILogger>(Mock.Of<ILogger>);
            NcqrsEnvironment.SetGetter<IUniqueIdentifierGenerator>(Mock.Of<IUniqueIdentifierGenerator>);
            NcqrsEnvironment.SetGetter<IClock>(Mock.Of<IClock>);
        }

        public void OnAssemblyComplete() { }
    }
}
