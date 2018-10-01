using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NHibernate;
using Npgsql;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;
using WB.Tests.Integration.PostgreSQLTests;

namespace WB.Tests.Integration.InterviewPackagesServiceTests
{
    internal class when_processing_package : with_postgres_db
    {
        [NUnit.Framework.OneTimeSetUp] public void context ()
        {
            var sessionFactory = IntegrationCreate.SessionFactory(ConnectionStringBuilder.ConnectionString, new[] { typeof(InterviewPackageMap), typeof(BrokenInterviewPackageMap) }, true);
            plainPostgresTransactionManager = new PlainPostgresTransactionManager(sessionFactory ?? Mock.Of<ISessionFactory>());

            pgSqlConnection = new NpgsqlConnection(ConnectionStringBuilder.ConnectionString);
            pgSqlConnection.Open();

            packagesStorage = new PostgresPlainStorageRepository<InterviewPackage>(plainPostgresTransactionManager);

            mockOfCommandService = new Mock<ICommandService>();
            mockOfCommandService.Setup(x => x.Execute(Moq.It.IsAny<SynchronizeInterviewEventsCommand>(), Moq.It.IsAny<string>()))
                .Callback<ICommand, string>((command, origin) => actualCommand = command as SynchronizeInterviewEventsCommand);

            origin = "hq";

            var newtonJsonSerializer = new JsonAllTypesSerializer();

            transactionManager = new Mock<ITransactionManager>();
            interviewPackagesService = Create.Service.InterviewPackagesService(
                syncSettings: new SyncSettings(origin) { UseBackgroundJobForProcessingPackages = true},
                logger: Mock.Of<ILogger>(),
                serializer: newtonJsonSerializer, 
                interviewPackageStorage: packagesStorage,
                brokenInterviewPackageStorage: Mock.Of<IPlainStorageAccessor<BrokenInterviewPackage>>(),
                commandService: mockOfCommandService.Object,
                uniqueKeyGenerator: Mock.Of<IInterviewUniqueKeyGenerator>(),
                interviews: new TestInMemoryWriter<InterviewSummary>(),
                transactionManager: transactionManager.Object);

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
                        new SupervisorAssigned(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.Now),
                        new DateTimeQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[] { 2, 5, 8}, DateTime.UtcNow, DateTime.Today),  
                    });

            plainPostgresTransactionManager.ExecuteInPlainTransaction(
                () => interviewPackagesService.StoreOrProcessPackage(
                    new InterviewPackage
                    {
                        InterviewId = expectedCommand.InterviewId,
                        QuestionnaireId = expectedCommand.QuestionnaireId,
                        QuestionnaireVersion = expectedCommand.QuestionnaireVersion,
                        ResponsibleId = expectedCommand.UserId,
                        InterviewStatus = expectedCommand.InterviewStatus,
                        IsCensusInterview = expectedCommand.CreatedOnClient,
                        Events = newtonJsonSerializer.Serialize(expectedCommand.SynchronizedEvents.Select(IntegrationCreate.AggregateRootEvent).ToArray())
                    }));
            BecauseOf();
        }

        private void BecauseOf() => plainPostgresTransactionManager.ExecuteInPlainTransaction(
                () => interviewPackagesService.ProcessPackage("1"));

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

        [NUnit.Framework.Test] public void should_commit_transaction () => transactionManager.Verify(x => x.CommitCommandTransaction(), Times.Once);
        [NUnit.Framework.Test] public void should_begin_transaction () => transactionManager.Verify(x => x.BeginCommandTransaction(), Times.Once);

        [OneTimeTearDown]
        public void TearDown() => pgSqlConnection.Close();

        private static SynchronizeInterviewEventsCommand expectedCommand;
        private static SynchronizeInterviewEventsCommand actualCommand;
        private static Mock<ICommandService> mockOfCommandService;
        private static InterviewPackagesService interviewPackagesService;
        private static PostgresPlainStorageRepository<InterviewPackage> packagesStorage;
        private static PlainPostgresTransactionManager plainPostgresTransactionManager;
        static NpgsqlConnection pgSqlConnection;
        private static string origin;
        private static Mock<ITransactionManager> transactionManager;
    }
}
