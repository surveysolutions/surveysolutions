using System;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.WebInterview.Services;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Headquarters.Code;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.WebInterview
{
    public class WebInterviewAllowServiceTests
    {
        private InterviewSummary interview;
        private WebInterviewAllowService webInterviewAllowService;
        private Guid interviewId;
        private Guid? interviewerId;

        private ITransactionManagerProvider transactionManagerProvider;
        private Mock<IQueryableReadSideRepositoryReader<InterviewSummary>> interviewSummaryRepoMock;
        private IWebInterviewConfigProvider webInterviewConfigProvider;
        private IPlainTransactionManagerProvider plainTransactionManagerProvider;
        private HqUserManager hqUserManager;
        private WebInterviewConfig webInterviewConfig;
        private Mock<IUserStore<HqUser, Guid>> mockOfUserManager;

        [SetUp]
        public void Setup()
        {
            var transactionManager = Mock.Of<ITransactionManager>();
            transactionManagerProvider = Mock.Of<ITransactionManagerProvider>(tmp => tmp.GetTransactionManager() == transactionManager);

            var plainTransactionManager = Mock.Of<IPlainTransactionManager>();
            this.plainTransactionManagerProvider = Mock.Of<IPlainTransactionManagerProvider>(tmp => tmp.GetPlainTransactionManager() == plainTransactionManager);

            interviewSummaryRepoMock = new Mock<IQueryableReadSideRepositoryReader<InterviewSummary>>();

            webInterviewConfig = new WebInterviewConfig();
            webInterviewConfigProvider = Mock.Of<IWebInterviewConfigProvider>(tmp => tmp.Get(It.IsAny<QuestionnaireIdentity>()) == webInterviewConfig);

            this.mockOfUserManager = new Mock<IUserStore<HqUser, Guid>>();

            hqUserManager = Create.Storage.HqUserManager(userStore: mockOfUserManager.Object);

            webInterviewAllowService = new WebInterviewAllowService(transactionManagerProvider, 
                plainTransactionManagerProvider,
                interviewSummaryRepoMock.Object, 
                webInterviewConfigProvider, 
                hqUserManager);
        }

        private void Act()
        {
            webInterviewAllowService.CheckWebInterviewAccessPermissions(interviewId.ToString(), interviewerId);
        }

        private void ArrangeTest(
            UserRoles? userRole = null, 
            InterviewStatus? interviewStatus = InterviewStatus.InterviewerAssigned, 
            bool webInterviewEnabled = true, 
            Guid? interviewerId = null, 
            Guid? responsibleId = null)
        {
            if (interviewerId.HasValue && userRole.HasValue)
            {
                mockOfUserManager.Setup(_ => _.FindByIdAsync(It.IsAny<Guid>()))
                    .Returns(Task.FromResult(Create.Entity.HqUser(userId: interviewerId, role: userRole.Value)));
            }
            
            hqUserManager = Create.Storage.HqUserManager(userStore: mockOfUserManager.Object);

            if (interviewStatus.HasValue)
            {                
                interview = Create.Entity.InterviewSummary(responsibleId: responsibleId ?? Guid.NewGuid(), status: interviewStatus.Value);
                
                webInterviewConfig.Started = webInterviewEnabled;
                
                if (interviewerId.HasValue)
                {
                    this.interviewerId = interviewerId.Value;
                }

                interviewSummaryRepoMock
                    .Setup(s => s.GetById(It.IsAny<string>()))
                    .Returns(interview);
            }
        }

        [TestCase(InterviewStatus.InterviewerAssigned, ExpectedResult = true)]
        [TestCase(InterviewStatus.Restarted, ExpectedResult = true)]
        [TestCase(InterviewStatus.Completed, ExpectedResult = false)]
        [TestCase(InterviewStatus.ApprovedByHeadquarters, ExpectedResult = false)]
        [TestCase(InterviewStatus.RejectedByHeadquarters, ExpectedResult = false)]
        public bool should_only_allow_interviews_in_state(InterviewStatus interviewStatus)
        {
            ArrangeTest(interviewStatus: interviewStatus, webInterviewEnabled: true);

            try
            {
                Act();
            }
            catch(WebInterviewAccessException)
            {
                return false;
            }
            return true;
        }

        [Test]
        public void should_not_allow_user_that_is_not_responsible()
        {
            ArrangeTest(UserRoles.Interviewer, InterviewStatus.InterviewerAssigned, 
                webInterviewEnabled: false, interviewerId: Id.g1, responsibleId:Id.g2);

            Assert.Throws<WebInterviewAccessException>(Act);
        }

        [Test]
        public void should_allow_access_if_webInterviewEnabled()
        {
            ArrangeTest(webInterviewEnabled: true);

            Act();
        }

        [TestCase(UserRoles.Interviewer, ExpectedResult = true)]
        [TestCase(UserRoles.Supervisor, ExpectedResult = false)]
        [TestCase(UserRoles.Headquarter, ExpectedResult = false)]
        public bool should_allow_access_for_interviewerOnly_when_webInterview_disabled(UserRoles userRole)
        {
            ArrangeTest(userRole, webInterviewEnabled: false, interviewerId: Id.g1, responsibleId: Id.g1);

            try
            {
                Act();
            }
            catch(WebInterviewAccessException)
            {
                return false;
            }
            return true;
        }

        [Test]
        public void should_not_allow_access_if_interview_is_not_exists()
        {
            ArrangeTest(interviewStatus: null);

            Assert.Throws<WebInterviewAccessException>(Act);
        }

        [Test]
        public void should_not_allow_access_for_deleted_interview()
        {
            ArrangeTest(interviewStatus: InterviewStatus.Deleted);

            Assert.Throws<WebInterviewAccessException>(Act);
        }
        
    }
}