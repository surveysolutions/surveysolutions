using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NHibernate;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;
using WB.Tests.Integration.PostgreSQLTests;

namespace WB.Tests.Integration.InterviewPackagesServiceTests
{
    internal class when_processing_package_failed : with_postgres_db
    {
        [OneTimeSetUp]
        public void context()
        {
            sessionFactory = IntegrationCreate.SessionFactory(ConnectionStringBuilder.ConnectionString,
                new[] { typeof(InterviewPackageMap), typeof(BrokenInterviewPackageMap) }, true);

            Setup.InstanceToMockedServiceLocator(sessionFactory);
            var UnitOfWork = IntegrationCreate.UnitOfWork(sessionFactory);

            var origin = "hq";
            expectedException = new InterviewException("Some interview exception",
                InterviewDomainExceptionType.StatusIsNotOneOfExpected);

            var packagesStorage = new PostgresPlainStorageRepository<InterviewPackage>(UnitOfWork);
            var brokenPackagesStorage = new PostgresPlainStorageRepository<BrokenInterviewPackage>(UnitOfWork);

            var mockOfCommandService = new Mock<ICommandService>();
            mockOfCommandService.Setup(x => x.Execute(Moq.It.IsAny<SynchronizeInterviewEventsCommand>(), Moq.It.IsAny<string>()))
                .Throws(expectedException);

            var newtonJsonSerializer = new JsonAllTypesSerializer();

            var interviewPackagesService = Create.Service.InterviewPackagesService(
                syncSettings: new SyncSettings(origin) { UseBackgroundJobForProcessingPackages = true },
                logger: Mock.Of<ILogger>(),
                serializer: newtonJsonSerializer,
                interviewPackageStorage: packagesStorage,
                brokenInterviewPackageStorage: brokenPackagesStorage,
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
                    new SupervisorAssigned(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.Now),
                    new DateTimeQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[] {2, 5, 8},
                        DateTime.UtcNow, DateTime.Today),
                });

            expectedEventsString = newtonJsonSerializer.Serialize(expectedCommand.SynchronizedEvents
                .Select(IntegrationCreate.AggregateRootEvent).ToArray());

            interviewPackagesService.StoreOrProcessPackage(new InterviewPackage
            {
                InterviewId = expectedCommand.InterviewId,
                QuestionnaireId = expectedCommand.QuestionnaireId,
                QuestionnaireVersion = expectedCommand.QuestionnaireVersion,
                ResponsibleId = expectedCommand.UserId,
                InterviewStatus = expectedCommand.InterviewStatus,
                IsCensusInterview = expectedCommand.CreatedOnClient,
                Events = expectedEventsString
            });

            interviewPackagesService.ProcessPackage("1");

            UnitOfWork.AcceptChanges();
        }
            

        [NUnit.Framework.Test]
        public void should_broken_packages_storage_contains_specified_interview()
        {
            var UoW = IntegrationCreate.UnitOfWork(sessionFactory);
            var brokenPackagesStorageVerifier = new PostgresPlainStorageRepository<BrokenInterviewPackage>(UoW);

            var expectedPackage = brokenPackagesStorageVerifier.GetById(1);

            expectedPackage.IsCensusInterview.Should().Be(expectedCommand.CreatedOnClient);
            expectedPackage.InterviewStatus.Should().Be(expectedCommand.InterviewStatus);
            expectedPackage.ResponsibleId.Should().Be(expectedCommand.UserId);
            expectedPackage.InterviewId.Should().Be(expectedCommand.InterviewId);
            expectedPackage.QuestionnaireId.Should().Be(expectedCommand.QuestionnaireId);
            expectedPackage.QuestionnaireVersion.Should().Be(expectedCommand.QuestionnaireVersion);
           // expectedPackage.ExceptionType.Should().Be(expectedException.ExceptionType.ToString());
           // expectedPackage.ExceptionMessage.Should().Be(expectedException.Message);
            expectedPackage.Events.Should().Be(expectedEventsString);
            expectedPackage.PackageSize.Should().Be(expectedEventsString.Length);

            UoW.Dispose();
        }


        private ISessionFactory sessionFactory;
        private SynchronizeInterviewEventsCommand expectedCommand;
        private InterviewException expectedException;
        private string expectedEventsString;
    }
}
