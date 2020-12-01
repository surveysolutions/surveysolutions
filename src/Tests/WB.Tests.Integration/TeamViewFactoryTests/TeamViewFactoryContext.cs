using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using NHibernate;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Workspaces;
using WB.Tests.Abc;
using WB.Tests.Integration.PostgreSQLEventStoreTests;

namespace WB.Tests.Integration.TeamViewFactoryTests
{
    class TeamViewFactoryContext
    {
        private string connectionString;
        protected IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryRepository;
        protected IUnitOfWork UnitOfWork;
        protected ISessionFactory sessionFactory;
        private IWorkspaceContextAccessor workspace;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            SetUp.MockedServiceLocator();

            this.connectionString = DatabaseTestInitializer.CreateAndInitializeDb(DbType.PlainStore, DbType.ReadSide);

            workspace = Create.Service.WorkspaceContextAccessor();
            
            sessionFactory = IntegrationCreate.SessionFactory(this.connectionString,
                new List<Type>
                {
                    typeof(InterviewSummaryMap),
                    typeof(InterviewGpsMap),
                    typeof(QuestionnaireCompositeItemMap),
                    typeof(IdentifyEntityValueMap),
                    typeof(InterviewStatisticsReportRowMap),
                    typeof(TimeSpanBetweenStatusesMap),
                    typeof(CumulativeReportStatusChangeMap),
                    typeof(InterviewCommentedStatusMap),
                    typeof(InterviewCommentMap)
                }, true, workspace.CurrentWorkspace().SchemaName);

            Abc.SetUp.InstanceToMockedServiceLocator<IEntitySerializer<int[][]>>(new EntitySerializer<int[][]>());
            Abc.SetUp.InstanceToMockedServiceLocator<IEntitySerializer<GeoPosition>>(new EntitySerializer<GeoPosition>());
            Abc.SetUp.InstanceToMockedServiceLocator<IEntitySerializer<InterviewTextListAnswer[]>>(new EntitySerializer<InterviewTextListAnswer[]>());
            Abc.SetUp.InstanceToMockedServiceLocator<IEntitySerializer<AnsweredYesNoOption[]>>(new EntitySerializer<AnsweredYesNoOption[]>());
            Abc.SetUp.InstanceToMockedServiceLocator<IEntitySerializer<AudioAnswer>>(new EntitySerializer<AudioAnswer>());
            Abc.SetUp.InstanceToMockedServiceLocator<IEntitySerializer<Area>>(new EntitySerializer<Area>());
        }

        [SetUp]
        public void EachTestSetup()
        {
            this.UnitOfWork = IntegrationCreate.UnitOfWork(sessionFactory);

            this.interviewSummaryRepository = IntegrationCreate.PostgresReadSideRepository<InterviewSummary>(UnitOfWork);
        }

        [TearDown]
        public void TearDown()
        {
            this.UnitOfWork.Dispose();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DatabaseTestInitializer.DropDb(this.connectionString);
        }

        protected void StoreInterviewSummary(InterviewSummary interviewSummary, QuestionnaireIdentity questionnaireIdentity)
        {
            interviewSummary.QuestionnaireIdentity = questionnaireIdentity.ToString();
            interviewSummary.SummaryId = interviewSummary.InterviewId.FormatGuid();

            var repository = IntegrationCreate.PostgresReadSideRepository<InterviewSummary>(UnitOfWork);
            repository.Store(interviewSummary, interviewSummary.SummaryId);
            UnitOfWork.Session.Flush();
        }

        protected ITeamViewFactory CreateTeamViewFactory(IUserRepository userRepository)
        {
            return new TeamViewFactory(
                interviewSummaryReader: interviewSummaryRepository,
                sessionProvider: this.UnitOfWork,
                userRepository: userRepository);
        }

        protected IUserRepository SetupUserRepositoryWithSupervisor(Guid supervisorId)
        {
            var usersRepository = new Mock<IUserRepository>();
            var supervisorRole = Create.Entity.HqRole(UserRoles.Supervisor);

            var supervisorUser = new HqUser()
            {
                Id = supervisorId,
                Profile = new HqUserProfile() { SupervisorId = Guid.NewGuid() },
            };
            supervisorUser.Roles.Add(supervisorRole);
            usersRepository.Setup(x => x.FindByIdAsync(supervisorId, It.IsAny<CancellationToken>())).Returns(Task.FromResult(supervisorUser));
            EnumerableQuery<HqUser> allUsers = new EnumerableQuery<HqUser>(new[] { supervisorUser });
            usersRepository.Setup(x => x.Users).Returns(allUsers);
            return usersRepository.Object;
        }
    }
}
