using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using NHibernate;
using Npgsql;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.Synchronization;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;
using WB.Tests.Integration.PostgreSQLTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewPackagesServiceTests
{
    internal class when_processing_package_failed : with_postgres_db
    {
        Establish context = () =>
        {
            var sessionFactory = IntegrationCreate.SessionFactory(connectionStringBuilder.ConnectionString, new[] { typeof(InterviewPackageMap), typeof(BrokenInterviewPackageMap) }, true);
            plainPostgresTransactionManager = new PlainPostgresTransactionManager(sessionFactory ?? Mock.Of<ISessionFactory>());

            origin = "hq";
            expectedException = new InterviewException("Some interview exception", InterviewDomainExceptionType.StatusIsNotOneOfExpected);

            pgSqlConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
            pgSqlConnection.Open();

            packagesStorage = new PostgresPlainStorageRepository<InterviewPackage>(plainPostgresTransactionManager);
            brokenPackagesStorage = new PostgresPlainStorageRepository<BrokenInterviewPackage>(plainPostgresTransactionManager);

            mockOfCommandService = new Mock<ICommandService>();
            mockOfCommandService.Setup(
                x => x.Execute(Moq.It.IsAny<SynchronizeInterviewEventsCommand>(), Moq.It.IsAny<string>()))
                .Throws(expectedException);

            var newtonJsonSerializer = new JsonAllTypesSerializer();

            transactionManagerMock = new Mock<ITransactionManager>();

            interviewPackagesService = new InterviewPackagesService(
                syncSettings: new SyncSettings(origin) {UseBackgroundJobForProcessingPackages = true},
                logger: Mock.Of<ILogger>(),
                serializer: newtonJsonSerializer, 
                interviewPackageStorage: packagesStorage,
                brokenInterviewPackageStorage: brokenPackagesStorage,
                commandService: mockOfCommandService.Object,
                uniqueKeyGenerator: Mock.Of<IInterviewUniqueKeyGenerator>(),
                interviews: new TestInMemoryWriter<InterviewSummary>(),
                transactionManager: transactionManagerMock.Object);

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
                        new InterviewerAssigned(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow),
                        new SupervisorAssigned(Guid.NewGuid(), Guid.NewGuid()),
                        new DateTimeQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[] { 2, 5, 8}, DateTime.UtcNow, DateTime.Today),  
                    });

            expectedEventsString = newtonJsonSerializer.Serialize(expectedCommand.SynchronizedEvents.Select(IntegrationCreate.AggregateRootEvent).ToArray());

            plainPostgresTransactionManager.ExecuteInPlainTransaction(
                () => interviewPackagesService.StoreOrProcessPackage(new InterviewPackage
                {
                    InterviewId = expectedCommand.InterviewId,
                    QuestionnaireId = expectedCommand.QuestionnaireId,
                    QuestionnaireVersion = expectedCommand.QuestionnaireVersion,
                    ResponsibleId = expectedCommand.UserId,
                    InterviewStatus = expectedCommand.InterviewStatus,
                    IsCensusInterview = expectedCommand.CreatedOnClient,
                    Events = expectedEventsString
                }));
        };

        Because of = () => plainPostgresTransactionManager.ExecuteInPlainTransaction(
                () => interviewPackagesService.ProcessPackage("1"));

        It should_broken_packages_storage_contains_specified_interview = () =>
        {
            var expectedPackage = plainPostgresTransactionManager.ExecuteInPlainTransaction(
                () => brokenPackagesStorage.GetById(1));

            expectedPackage.IsCensusInterview.ShouldEqual(expectedCommand.CreatedOnClient);
            expectedPackage.InterviewStatus.ShouldEqual(expectedCommand.InterviewStatus);
            expectedPackage.ResponsibleId.ShouldEqual(expectedCommand.UserId);
            expectedPackage.InterviewId.ShouldEqual(expectedCommand.InterviewId);
            expectedPackage.QuestionnaireId.ShouldEqual(expectedCommand.QuestionnaireId);
            expectedPackage.QuestionnaireVersion.ShouldEqual(expectedCommand.QuestionnaireVersion);
            expectedPackage.ExceptionType.ShouldEqual(expectedException.ExceptionType.ToString());
            expectedPackage.ExceptionMessage.ShouldEqual(expectedException.Message);
            expectedPackage.Events.ShouldEqual(expectedEventsString);
            expectedPackage.PackageSize.ShouldEqual(expectedEventsString.Length);
        };

        It should_rollback_transaction = () => transactionManagerMock.Verify(x => x.BeginCommandTransaction(), Times.Once);

        Cleanup things = () => { pgSqlConnection.Close(); };

        private static SynchronizeInterviewEventsCommand expectedCommand;
        private static Mock<ICommandService> mockOfCommandService;
        private static InterviewPackagesService interviewPackagesService;
        private static PostgresPlainStorageRepository<InterviewPackage> packagesStorage;
        private static PostgresPlainStorageRepository<BrokenInterviewPackage> brokenPackagesStorage;
        private static PlainPostgresTransactionManager plainPostgresTransactionManager;
        static NpgsqlConnection pgSqlConnection;
        private static string origin;
        private static InterviewException expectedException;
        private static string expectedEventsString;
        private static Mock<ITransactionManager> transactionManagerMock;
    }
}