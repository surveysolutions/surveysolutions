using System;
using System.Globalization;
using System.Threading;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;

namespace WB.Tests.Integration
{
    public class AssemblyContext : IAssemblyContext
    {
        public void OnAssemblyStart()
        {
            Setup.MockedServiceLocator();
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

            Setup.InstanceToMockedServiceLocator<ISubstitutionService>(new SubstitutionService());
            Setup.InstanceToMockedServiceLocator<IKeywordsProvider>(new KeywordsProvider(new SubstitutionService()));
            Setup.InstanceToMockedServiceLocator<IFileSystemAccessor>(new FileSystemIOAccessor());

            Setup.InstanceToMockedServiceLocator<ILogger>(Mock.Of<ILogger>());
            Setup.InstanceToMockedServiceLocator<IClock>(Mock.Of<IClock>());

            Setup.InstanceToMockedServiceLocator<IQuestionOptionsRepository>(new QuestionnaireQuestionOptionsRepository());
        }

        internal static class Stub<T> where T : class
        {
            public static T WithNotEmptyValues => new Mock<T> { DefaultValue = DefaultValue.Mock }.Object;
        }
    }
}
