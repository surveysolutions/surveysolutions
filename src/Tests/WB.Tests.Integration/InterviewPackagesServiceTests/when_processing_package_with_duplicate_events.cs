using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using FluentAssertions;
using Main.Core.Events;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using ReflectionMagic;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
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
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Integration.InterviewPackagesServiceTests
{
    internal class when_processing_package_with_duplicate_events : with_postgres_db
    {
        [Test]
        public void should_correct_resolve_duplicate_events_in_stream()
        {
            var userId = Id.g1;
            var supervisorId = Id.g2;
            var interviewId = Id.g3;
            var questionnaireId = Id.g4;
            var questionnaireVersion = 111;

            var sessionFactory = IntegrationCreate.SessionFactory(ConnectionStringBuilder.ConnectionString,
                new[] {typeof(InterviewPackageMap), typeof(BrokenInterviewPackageMap)}, true);
            var plainPostgresTransactionManager = Mock.Of<IUnitOfWork>(x => x.Session == sessionFactory.OpenSession());

            var packagesStorage = new PostgresPlainStorageRepository<InterviewPackage>(plainPostgresTransactionManager);

            SynchronizeInterviewEventsCommand actualCommand = null;
            var mockOfCommandService = new Mock<ICommandService>();
            mockOfCommandService.Setup(x =>
                    x.Execute(Moq.It.IsAny<SynchronizeInterviewEventsCommand>(), Moq.It.IsAny<string>()))
                .Callback<ICommand, string>((command, origin) =>
                    actualCommand = command as SynchronizeInterviewEventsCommand);

            var origin = "super";

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

            serviceLocatorNestedMock
                .Setup(x => x.GetInstance<IPlainStorageAccessor<ReceivedPackageLogEntry>>())
                .Returns(packageStore.Object);
            serviceLocatorNestedMock
                .Setup(x => x.GetInstance<IInterviewUniqueKeyGenerator>())
                .Returns(Mock.Of<IInterviewUniqueKeyGenerator>);

            var duplicate1 = Create.Event.InterviewReceivedBySupervisor(DateTimeOffset.UtcNow.AddMinutes(-5)).ToCommittedEvent(eventId: Id.g7);
            var duplicate2 = Create.Event.InterviewReceivedByInterviewer(DateTimeOffset.UtcNow).ToCommittedEvent(eventId: Id.g8);

            var hqEvents = new CommittedEvent[]
            {
                Create.Event.InterviewOnClientCreated(questionnaireId, questionnaireVersion).ToCommittedEvent(),
                Create.Event.InterviewerAssigned(userId, userId, DateTimeOffset.Now).ToCommittedEvent(),
                Create.Event.SupervisorAssigned(userId, supervisorId, DateTimeOffset.Now).ToCommittedEvent(),
                Create.Event.DateTimeQuestionAnswered(Guid.NewGuid(), DateTime.UtcNow, new decimal[] {2, 5, 8}).ToCommittedEvent(),
                duplicate1,
                duplicate2,
            };
            var headquartersEventStore = Mock.Of<IHeadquartersEventStore>(es =>
                es.Read(interviewId, 0) == hqEvents);
            serviceLocatorNestedMock
                .Setup(x => x.GetInstance<IHeadquartersEventStore>())
                .Returns(headquartersEventStore);

            var users = new Mock<IUserRepository>();
            var hqUser = new HqUser() {WorkspaceProfile = new WorkspaceUserProfile()};
            hqUser.WorkspaceProfile.AsDynamic().SupervisorId = supervisorId;
            
            users.Setup(x => x.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(hqUser));

            serviceLocatorNestedMock.Setup(x => x.GetInstance<IUserRepository>()).Returns(users.Object);

            var executor = new NoScopeInScopeExecutor(serviceLocatorNestedMock.Object);

            var interviewPackagesService = Create.Service.InterviewPackagesService(
                    syncSettings: new SyncSettings(origin) {},
                    logger: Mock.Of<ILogger>(),
                    serializer: newtonJsonSerializer,
                    interviewPackageStorage: packagesStorage,
                    brokenInterviewPackageStorage: Mock.Of<IPlainStorageAccessor<BrokenInterviewPackage>>(),
                    commandService: mockOfCommandService.Object,
                    uniqueKeyGenerator: Mock.Of<IInterviewUniqueKeyGenerator>(),
                    interviews: new TestInMemoryWriter<InterviewSummary>(),
                    inScopeExecutor: executor);

            // package with duplicates
            var interviewPackageSecond = new InterviewPackage
            {
                InterviewId = interviewId,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                ResponsibleId = userId,
                InterviewStatus = InterviewStatus.Completed,
                IsCensusInterview = true,
                Events = newtonJsonSerializer.Serialize(new AggregateRootEvent[]
                {
                    new(duplicate1), // duplicate #1
                    new(duplicate2), // duplicate #2
                    Create.Event.TextQuestionAnswered(userId: userId).ToAggregateRootEvent(),
                    Create.Event.InterviewCompleted().ToAggregateRootEvent(),
                })
            };
            interviewPackagesService.ProcessPackage(interviewPackageSecond);

            mockOfCommandService.Verify(x => x.Execute(Moq.It.IsAny<SynchronizeInterviewEventsCommand>(), origin), Times.Once);

            actualCommand.CreatedOnClient.Should().Be(true);
            actualCommand.InterviewStatus.Should().Be(InterviewStatus.Completed);
            actualCommand.QuestionnaireId.Should().Be(questionnaireId);
            actualCommand.QuestionnaireVersion.Should().Be(questionnaireVersion);
            var commandEvents = actualCommand.SynchronizedEvents;
            commandEvents.Length.Should().Be(2);
            commandEvents[0].Payload.GetType().Should().Be(typeof(TextQuestionAnswered));
            commandEvents[1].Payload.GetType().Should().Be(typeof(InterviewCompleted));
            
            plainPostgresTransactionManager.Dispose();
        }
    }
}
