using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.WebInterview.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Headquarters.Code;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.WebInterview
{
    [TestFixture]
    [TestOf(typeof(WebInterviewAllowService))]
    public class WebInterviewAllowServiceTests
    {
        private InterviewSummary interview;
        private WebInterviewAllowService webInterviewAllowService;
        private Guid interviewId;

        private ITransactionManagerProvider transactionManagerProvider;
        private Mock<IQueryableReadSideRepositoryReader<InterviewSummary>> interviewSummaryRepoMock;
        private IWebInterviewConfigProvider webInterviewConfigProvider;
        private IPlainTransactionManagerProvider plainTransactionManagerProvider;
        private WebInterviewConfig webInterviewConfig;

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

            webInterviewAllowService = new WebInterviewAllowService(transactionManagerProvider, 
                plainTransactionManagerProvider,
                interviewSummaryRepoMock.Object, 
                webInterviewConfigProvider);
        }

        [TearDown]
        public void TearDown()
        {
            Thread.CurrentPrincipal = null;
        }

        private void Act()
        {
            webInterviewAllowService.CheckWebInterviewAccessPermissions(interviewId.ToString());
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
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, interviewerId.ToString()),
                    new Claim(ClaimTypes.Role, userRole.ToString())
                };

                Thread.CurrentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
            }

            if (interviewStatus.HasValue)
            {                
                interview = Create.Entity.InterviewSummary(responsibleId: responsibleId ?? Guid.NewGuid(), status: interviewStatus.Value);
                
                webInterviewConfig.Started = webInterviewEnabled;
                
                if (interviewerId.HasValue)
                {
                }

                interviewSummaryRepoMock
                    .Setup(s => s.GetById(It.IsAny<string>()))
                    .Returns(interview);
            }
        }

        [TestCase(InterviewStatus.InterviewerAssigned, ExpectedResult = true)]
        [TestCase(InterviewStatus.Restarted, ExpectedResult = false)]
        [TestCase(InterviewStatus.Completed, ExpectedResult = false)]
        [TestCase(InterviewStatus.ApprovedByHeadquarters, ExpectedResult = false)]
        [TestCase(InterviewStatus.RejectedByHeadquarters, ExpectedResult = false)]
        [TestCase(InterviewStatus.RejectedBySupervisor, ExpectedResult = false)]
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

        [Test]
        public void should_allow_for_interviewer_when_interview_is_rejected()
        {
            ArrangeTest(UserRoles.Interviewer, InterviewStatus.RejectedBySupervisor,
                webInterviewEnabled: false, interviewerId: Id.g1, responsibleId: Id.g1);
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