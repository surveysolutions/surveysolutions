using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Main.Core.Events;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;
using WB.UI.Shared.Enumerator.Services.Internals;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewPackagesServiceTests
{
    [TestFixture]
    public class when_processing_package
    {
        [OneTimeSetUp]
        public void Setup()
        {
            var serializer =
                Mock.Of<IJsonAllTypesSerializer>(x => x.Deserialize<SyncItem>(It.IsAny<string>()) == new SyncItem() &&
                                          x.Deserialize<InterviewMetaInfo>(It.IsAny<string>()) == new InterviewMetaInfo { Status = 0 } &&
                                          x.Deserialize<AggregateRootEvent[]>(decompressedEvents) == new AggregateRootEvent[0]);
            var syncSettings = Mock.Of<SyncSettings>(x => x.UseBackgroundJobForProcessingPackages == true);

            brokenPackagesStorage = new TestPlainStorage<BrokenInterviewPackage>();
            packagesStorage = new TestPlainStorage<InterviewPackage>();
            
            mockOfCommandService = new Mock<ICommandService>();

            

            var newtonJsonSerializer = new JsonAllTypesSerializer();

            IComponentRegistration componentRegistration = new Mock<IComponentRegistration>().Object;
            var componentRegistry = new Mock<IComponentRegistry>();
            componentRegistry.Setup(x =>
                    x.TryGetRegistration(It.IsAny<Service>(), out componentRegistration))
                .Returns(true);

            var container = new Mock<ILifetimeScope>();
            container.Setup(x => x.BeginLifetimeScope()).Returns(container.Object);
            container.SetupGet(x => x.ComponentRegistry).Returns(componentRegistry.Object);

            var serviceLocatorNestedMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            serviceLocatorNestedMock.Setup(x => x.GetInstance<ICommandService>()).Returns(mockOfCommandService.Object);
            serviceLocatorNestedMock.Setup(x => x.GetInstance<IJsonAllTypesSerializer>())
                .Returns(serializer);

            var packageStore = new Mock<IPlainStorageAccessor<ReceivedPackageLogEntry>>();
            packageStore.Setup(x => x.Query(It.IsAny<Func<IQueryable<ReceivedPackageLogEntry>, ReceivedPackageLogEntry>>())).Returns((ReceivedPackageLogEntry)null);

            serviceLocatorNestedMock.Setup(x => x.GetInstance<IPlainStorageAccessor<ReceivedPackageLogEntry>>()).Returns(packageStore.Object);
            serviceLocatorNestedMock.Setup(x => x.GetInstance<IInterviewUniqueKeyGenerator>()).Returns(Mock.Of<IInterviewUniqueKeyGenerator>);

            var users = new Mock<IUserRepository>();
            users.Setup(x => x.FindByIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult(new HqUser() { Profile = new HqUserProfile() { /*SupervisorId = supervisorId*/ } }));

            serviceLocatorNestedMock.Setup(x => x.GetInstance<IUserRepository>()).Returns(users.Object);

            container.Setup(x => x.ResolveComponent(It.IsAny<IComponentRegistration>(), It.IsAny<System.Collections.Generic.IEnumerable<Autofac.Core.Parameter>>()))
                .Returns((IComponentRegistration compRegistration, IEnumerable<Autofac.Core.Parameter> pars) =>
                {
                    return serviceLocatorNestedMock.Object;
                });

            var autofacServiceLocatorAdapterForTests = new AutofacServiceLocatorAdapter(container.Object);

            var serviceLocatorOriginal = ServiceLocator.IsLocationProviderSet ? ServiceLocator.Current : null;
            ServiceLocator.SetLocatorProvider(() => autofacServiceLocatorAdapterForTests);

            interviewPackagesService = Create.Service.InterviewPackagesService(
                serializer: serializer, brokenInterviewPackageStorage: brokenPackagesStorage,
                interviewPackageStorage: packagesStorage, commandService: mockOfCommandService.Object,
                syncSettings: syncSettings);
            

            interviewPackagesService.StoreOrProcessPackage(new InterviewPackage
            {
                InterviewId = Guid.Parse("11111111111111111111111111111111"),
                QuestionnaireId = Guid.Parse("22222222222222222222222222222222"),
                QuestionnaireVersion = 111,
                ResponsibleId = Guid.Parse("33333333333333333333333333333333"),
                InterviewStatus = InterviewStatus.Restarted,
                IsCensusInterview = false,
                Events = "compressed serialized events"
            });

            interviewPackagesService.ProcessPackage("1");

            ServiceLocator.SetLocatorProvider(() => serviceLocatorOriginal);
        }
        
        [Test]
        public void should_execute_SynchronizeInterviewEventsCommand_command() => 
            mockOfCommandService.Verify(x => x.Execute(It.IsAny<SynchronizeInterviewEventsCommand>(), It.IsAny<string>()), Times.Once);

        private static Mock<ICommandService> mockOfCommandService;
        private static string decompressedEvents = "decompressed events";
        private static InterviewPackagesService interviewPackagesService;
        private static IPlainStorageAccessor<BrokenInterviewPackage> brokenPackagesStorage;
        private static IPlainStorageAccessor<InterviewPackage> packagesStorage;
    }
}
