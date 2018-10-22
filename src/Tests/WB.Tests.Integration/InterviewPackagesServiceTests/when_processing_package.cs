using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using FluentAssertions;
using Moq;
using Npgsql;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;
using WB.Tests.Integration.PostgreSQLTests;
using WB.UI.Shared.Enumerator.Services.Internals;

namespace WB.Tests.Integration.InterviewPackagesServiceTests
{
    internal class when_processing_package : with_postgres_db
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            var supervisorId = Guid.NewGuid();

            var sessionFactory = IntegrationCreate.SessionFactory(ConnectionStringBuilder.ConnectionString,
                new[] {typeof(InterviewPackageMap), typeof(BrokenInterviewPackageMap)}, true);
            plainPostgresTransactionManager = Mock.Of<IUnitOfWork>(x => x.Session == sessionFactory.OpenSession());

            packagesStorage = new PostgresPlainStorageRepository<InterviewPackage>(plainPostgresTransactionManager);

            mockOfCommandService = new Mock<ICommandService>();
            mockOfCommandService.Setup(x =>
                    x.Execute(Moq.It.IsAny<SynchronizeInterviewEventsCommand>(), Moq.It.IsAny<string>()))
                .Callback<ICommand, string>((command, origin) =>
                    actualCommand = command as SynchronizeInterviewEventsCommand);

            origin = "hq";

            var newtonJsonSerializer = new JsonAllTypesSerializer();

            IComponentRegistration componentRegistration = new Mock<IComponentRegistration>().Object;
            var componentRegistry = new Mock<IComponentRegistry>();
            componentRegistry.Setup(x =>
                    x.TryGetRegistration(It.IsAny<Service>(), out componentRegistration))
                .Returns(true);

            var container = new Mock<ILifetimeScope>();
            container.Setup(x => x.BeginLifetimeScope(It.IsAny<string>())).Returns(container.Object);
            container.SetupGet(x => x.ComponentRegistry).Returns(componentRegistry.Object);

            var serviceLocatorNestedMock = new Mock<IServiceLocator> {DefaultValue = DefaultValue.Mock};
            serviceLocatorNestedMock.Setup(x => x.GetInstance<ICommandService>()).Returns(mockOfCommandService.Object);
            serviceLocatorNestedMock.Setup(x => x.GetInstance<IJsonAllTypesSerializer>())
                .Returns(newtonJsonSerializer);

            var packageStore = new Mock<IPlainStorageAccessor<ReceivedPackageLogEntry>>();
            packageStore
                .Setup(x => x.Query(It.IsAny<Func<IQueryable<ReceivedPackageLogEntry>, ReceivedPackageLogEntry>>()))
                .Returns((ReceivedPackageLogEntry) null);

            serviceLocatorNestedMock.Setup(x => x.GetInstance<IPlainStorageAccessor<ReceivedPackageLogEntry>>())
                .Returns(packageStore.Object);
            serviceLocatorNestedMock.Setup(x => x.GetInstance<IInterviewUniqueKeyGenerator>())
                .Returns(Mock.Of<IInterviewUniqueKeyGenerator>);

            var users = new Mock<IUserRepository>();
            users.Setup(x => x.FindByIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult(new HqUser()
                {Profile = new HqUserProfile() {SupervisorId = supervisorId}}));

            serviceLocatorNestedMock.Setup(x => x.GetInstance<IUserRepository>()).Returns(users.Object);

            container.Setup(x => x.ResolveComponent(It.IsAny<IComponentRegistration>(),
                    It.IsAny<System.Collections.Generic.IEnumerable<Autofac.Core.Parameter>>()))
                .Returns((IComponentRegistration compRegistration, IEnumerable<Autofac.Core.Parameter> pars) =>
                    serviceLocatorNestedMock.Object);

            //container.Setup(x => x.Resolve<IServiceLocator>()).Returns(serviceLocatorNestedMock.Object);

            var autofacServiceLocatorAdapterForTests = new AutofacServiceLocatorAdapter(container.Object);

            var serviceLocatorOriginal = ServiceLocator.IsLocationProviderSet ? ServiceLocator.Current : null;

            ServiceLocator.SetLocatorProvider(() => autofacServiceLocatorAdapterForTests);
            try
            {
                interviewPackagesService = Create.Service.InterviewPackagesService(
                    syncSettings: new SyncSettings(origin) {UseBackgroundJobForProcessingPackages = true},
                    logger: Mock.Of<ILogger>(),
                    serializer: newtonJsonSerializer,
                    interviewPackageStorage: packagesStorage,
                    brokenInterviewPackageStorage: Mock.Of<IPlainStorageAccessor<BrokenInterviewPackage>>(),
                    commandService: mockOfCommandService.Object,
                    uniqueKeyGenerator: Mock.Of<IInterviewUniqueKeyGenerator>(),
                    interviews: new TestInMemoryWriter<InterviewSummary>());

                expectedCommand = Create.Command.SynchronizeInterviewEventsCommand(
                    interviewId: Guid.Parse("11111111111111111111111111111111"),
                    questionnaireId: Guid.Parse("22222222222222222222222222222222"),
                    questionnaireVersion: 111,
                    userId: Guid.Parse("33333333333333333333333333333333"),
                    interviewStatus: InterviewStatus.Restarted,
                    createdOnClient: true,
                    synchronizedEvents:
                    new IEvent[]
                    {
                        Create.Event.InterviewOnClientCreated(Guid.NewGuid(), 111),
                        new InterviewerAssigned(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.Now),
                        new SupervisorAssigned(Guid.NewGuid(), supervisorId, DateTimeOffset.Now),
                        new DateTimeQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[] {2, 5, 8},
                            DateTime.UtcNow, DateTime.Today),
                    });

                interviewPackagesService.StoreOrProcessPackage(
                    new InterviewPackage
                    {
                        InterviewId = expectedCommand.InterviewId,
                        QuestionnaireId = expectedCommand.QuestionnaireId,
                        QuestionnaireVersion = expectedCommand.QuestionnaireVersion,
                        ResponsibleId = expectedCommand.UserId,
                        InterviewStatus = expectedCommand.InterviewStatus,
                        IsCensusInterview = expectedCommand.CreatedOnClient,
                        Events = newtonJsonSerializer.Serialize(expectedCommand.SynchronizedEvents
                            .Select(IntegrationCreate.AggregateRootEvent).ToArray())
                    });

                BecauseOf();
            }
            finally
            {
                ServiceLocator.SetLocatorProvider(() => serviceLocatorOriginal);
            }
        }

        private void BecauseOf() => interviewPackagesService.ProcessPackage("1");

        [NUnit.Framework.Test] public void should_execute_SynchronizeInterviewEventsCommand_command () =>
            mockOfCommandService.Verify(x => x.Execute(Moq.It.IsAny<SynchronizeInterviewEventsCommand>(), origin), Times.Once);

        [NUnit.Framework.Test] public void should_actual_command_contains_specified_properties ()
        {
            actualCommand.CreatedOnClient.Should().Be(expectedCommand.CreatedOnClient);
            actualCommand.InterviewStatus.Should().Be(expectedCommand.InterviewStatus);
            actualCommand.QuestionnaireId.Should().Be(expectedCommand.QuestionnaireId);
            actualCommand.QuestionnaireVersion.Should().Be(expectedCommand.QuestionnaireVersion);
            actualCommand.SynchronizedEvents.Length.Should().Be(expectedCommand.SynchronizedEvents.Length);
            actualCommand.SynchronizedEvents.Should().OnlyContain(x=>expectedCommand.SynchronizedEvents.Any(y=>y.GetType() == x.GetType()));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            plainPostgresTransactionManager.Dispose();
        }

        private static SynchronizeInterviewEventsCommand expectedCommand;
        private static SynchronizeInterviewEventsCommand actualCommand;
        private static Mock<ICommandService> mockOfCommandService;
        private static InterviewPackagesService interviewPackagesService;
        private static PostgresPlainStorageRepository<InterviewPackage> packagesStorage;
        private static IUnitOfWork plainPostgresTransactionManager;
        private static string origin;
    }
}
