using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

using WB.Tests.Web.TestFactories;
using WB.UI.Headquarters.API.PublicApi;
using WB.UI.Headquarters.Code.CommandTransformation;
using WB.UI.Headquarters.Services;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    public class BaseAssignmentsControllerTest
    {
        protected AssignmentsController controller;

        protected Mock<IAssignmentsService> assignmentsStorage;
        protected Mock<IAssignmentViewFactory> assignmentViewFactory;
        protected Mock<IMapper> mapper;
        protected Mock<TestHqUserManager> userManager;
        protected Mock<IQuestionnaireStorage> questionnaireStorage;
        protected Mock<ILogger> logger;
        protected Mock<IPreloadedDataVerifier> interviewImportService;
        protected Mock<ICommandService> commandService;
        protected Mock<IAuthorizedUser> authorizedUser;

        [SetUp]
        public virtual void Setup()
        {
            this.PrepareMocks();

            var assignment = Create.Entity.Assignment();
            var assignmentsService = Mock.Of<IAssignmentsService>(s =>
                s.GetAssignmentByAggregateRootId(It.IsAny<Guid>()) == assignment);

            this.controller = new AssignmentsController(
                this.assignmentViewFactory.Object,
                this.assignmentsStorage.Object,
                this.mapper.Object,
                this.userManager.Object,
                this.logger.Object,
                this.questionnaireStorage.Object,
                Mock.Of<ISystemLog>(),
                Mock.Of<IInterviewCreatorFromAssignment>(),
                this.interviewImportService.Object,
                Mock.Of<ICommandTransformator>(),
                Create.Service.AssignmentFactory(commandService.Object, assignmentsService),
                Mock.Of<IInvitationService>(),
                Mock.Of<IAssignmentPasswordGenerator>(),
                commandService.Object,
                authorizedUser.Object);

            this.controller.Request = new HttpRequestMessage();
            this.controller.Configuration = new HttpConfiguration();
        }

        private void PrepareMocks()
        {
            this.assignmentsStorage = new Mock<IAssignmentsService>();
            this.assignmentViewFactory = new Mock<IAssignmentViewFactory>();
            this.mapper = new Mock<IMapper>();
            this.userManager = new Mock<TestHqUserManager>();
            this.interviewImportService = new Mock<IPreloadedDataVerifier>();
            this.questionnaireStorage = new Mock<IQuestionnaireStorage>();
            this.logger = new Mock<ILogger>();
            this.commandService = new Mock<ICommandService>();
            this.authorizedUser = new Mock<IAuthorizedUser>();
        }

        protected void SetupResponsibleUser(HqUser user)
        {
            this.userManager.Setup(um => um.FindByNameAsync(It.IsAny<string>())).Returns(Task.FromResult(user));
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
