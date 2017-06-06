using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using WB.Tests.Abc.TestFactories;
using WB.UI.Headquarters.API.PublicApi;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    public class BaseAssignmentsControllerTest
    {
        protected AssignmentsController controller;

        protected Mock<IPlainStorageAccessor<Assignment>> assignmentsStorage;
        protected Mock<IAssignmentViewFactory> assignmentViewFactory;
        protected Mock<IMapper> mapper;
        protected Mock<TestHqUserManager> userManager;
        protected Mock<IPreloadedDataVerifier> preloadedDataVerifier;
        protected Mock<IQuestionnaireStorage> questionnaireStorage;
        protected Mock<ILogger> logger;

        [SetUp]
        public virtual void Setup()
        {
            this.PrepareMocks();

            this.controller = new AssignmentsController(
                this.assignmentViewFactory.Object,
                this.assignmentsStorage.Object,
                this.preloadedDataVerifier.Object,
                this.mapper.Object,
                this.userManager.Object,
                this.logger.Object,
                this.questionnaireStorage.Object);

            this.controller.Request = new HttpRequestMessage();
            this.controller.Configuration = new HttpConfiguration();
        }

        private void PrepareMocks()
        {
            this.assignmentsStorage = new Mock<IPlainStorageAccessor<Assignment>>();
            this.assignmentViewFactory = new Mock<IAssignmentViewFactory>();
            this.mapper = new Mock<IMapper>();
            this.userManager = new Mock<TestHqUserManager>();
            this.preloadedDataVerifier = new Mock<IPreloadedDataVerifier>();
            this.questionnaireStorage = new Mock<IQuestionnaireStorage>();
            this.logger = new Mock<ILogger>();
        }

        protected void SetupResponsibleUser(HqUser user)
        {
            this.userManager.Setup(um => um.FindByNameAsync(It.IsAny<string>())).Returns(Task.FromResult(user));
        }
        
        protected void SetupAssignment(Assignment assignment)
        {
            this.assignmentsStorage.Setup(ass => ass.GetById(It.IsAny<int>())).Returns(assignment);
        }

        protected void SetupQuestionnaire(QuestionnaireDocument document)
        {
            var doc = Create.Entity.PlainQuestionnaire(document);
            this.questionnaireStorage
                .Setup(x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>())).Returns(doc);
        }
    }
}