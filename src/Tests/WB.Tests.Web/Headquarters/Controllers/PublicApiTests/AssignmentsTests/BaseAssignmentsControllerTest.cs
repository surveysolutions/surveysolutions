using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Tests.Abc;
using WB.UI.Headquarters.Controllers.Api.PublicApi;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    public class BaseAssignmentsControllerTest
    {
        protected AssignmentsController controller;

        protected Mock<IAssignmentsService> assignmentsStorage;
        protected Mock<IAssignmentViewFactory> assignmentViewFactory;
        protected Mock<IMapper> mapper;
        protected Mock<IUserRepository> userManager;
        protected Mock<IQuestionnaireStorage> questionnaireStorage;
        protected Mock<ILogger> logger;
        protected Mock<ICommandService> commandService;
        protected Mock<IAuthorizedUser> authorizedUser;
        protected Mock<IUserViewFactory> userViewFactory;

        [SetUp]
        public virtual void Setup()
        {
            this.PrepareMocks();

            var assignment = Create.Entity.Assignment();
            var assignmentsService = Mock.Of<IAssignmentsService>(s =>
                s.GetAssignmentByAggregateRootId(It.IsAny<Guid>()) == assignment);
            var assignmentFactory = Create.Service.AssignmentFactory(commandService.Object, assignmentsService);


            var sl = Mock.Of<IServiceLocator>(x => x.GetInstance<IAssignmentsService>() == assignmentsStorage.Object);
            var scopeExecutor = Create.Service.InScopeExecutor(sl);
            
            this.controller = new AssignmentsController(
                this.assignmentViewFactory.Object,
                this.assignmentsStorage.Object,
                this.mapper.Object,
                this.userManager.Object,
                this.questionnaireStorage.Object,
                Mock.Of<ISystemLog>(),
                Create.Service.ImportDataVerifier(),
                commandService.Object,
                authorizedUser.Object,
                Mock.Of<IUnitOfWork>(),
                this.userViewFactory.Object,
                Create.Service.AssignmentsImportService(assignmentFactory: assignmentFactory,
                    verifier: Create.Service.ImportDataVerifier(userViewFactory: userViewFactory.Object)),
                Create.Service.NewtonJsonSerializer(),
                Mock.Of<IInvitationService>(),
                Mock.Of<IWebInterviewLinkProvider>(),
                scopeExecutor);
        }

        private void PrepareMocks()
        {
            this.assignmentsStorage = new Mock<IAssignmentsService>();
            this.assignmentViewFactory = new Mock<IAssignmentViewFactory>();
            this.mapper = new Mock<IMapper>();
            this.userManager = new Mock<IUserRepository>();
            this.questionnaireStorage = new Mock<IQuestionnaireStorage>();
            this.logger = new Mock<ILogger>();
            this.commandService = new Mock<ICommandService>();
            this.authorizedUser = new Mock<IAuthorizedUser>();
            this.userViewFactory = new Mock<IUserViewFactory>();
        }

        protected void SetupResponsibleUser(HqUser user)
        {
            this.userViewFactory.Setup(uf => uf.GetUsersByUserNames(new[] {user.UserName})).Returns(new[]
            {
                new UserToVerify
                {
                    HeadquartersId = user.Roles.Any(x=>x.Name == "Headquarters") ? user.Id : (Guid?)null,
                    SupervisorId = user.Roles.Any(x=>x.Name == "Supervisor") ? user.Id : (Guid?)null,
                    InterviewerId = user.Roles.Any(x=>x.Name == "Interviewer") ? user.Id : (Guid?)null,
                    IsLocked = user.IsLockedByHeadquaters || user.IsLockedBySupervisor
                }
            });
            this.userManager.Setup(um => um.FindByNameAsync(It.IsAny<string>(), CancellationToken.None)).Returns(Task.FromResult(user));
        }
        
        protected void SetupAssignment(Assignment assignment)
        {
            this.assignmentsStorage.Setup(ass => ass.GetAssignment(It.IsAny<int>())).Returns(assignment);
        }

        protected void SetupQuestionnaire(QuestionnaireDocument document)
        {
            var doc = Create.Entity.PlainQuestionnaire(document);
            this.questionnaireStorage
                .Setup(x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>())).Returns(doc);
        }
    }
}
