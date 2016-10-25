using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;

namespace PerformanceTest
{
    internal static class Setup
    {
        public static void MockedServiceLocator()
        {
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };

            IExpressionProcessor roslynExpressionProcessor = new RoslynExpressionProcessor();

            serviceLocatorMock
                .Setup(locator => locator.GetInstance<IExpressionProcessor>())
                .Returns(roslynExpressionProcessor);

            var fileSystemIoAccessor = new FileSystemIOAccessor();

            serviceLocatorMock
                .Setup(locator => locator.GetInstance<IFileSystemAccessor>())
                .Returns(fileSystemIoAccessor);

            var questionOptionsRepository = new QuestionnaireQuestionOptionsRepository();

            serviceLocatorMock
                .Setup(locator => locator.GetInstance<IQuestionOptionsRepository>())
                .Returns(questionOptionsRepository);

            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);
        }

        public static void InstanceToMockedServiceLocator<TInstance>(TInstance instance)
        {
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<TInstance>())
                .Returns(instance);
        }
    }
}