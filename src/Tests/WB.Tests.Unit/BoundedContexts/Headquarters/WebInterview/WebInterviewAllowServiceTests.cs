using System;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.WebInterview.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Enumerator.Native.WebInterview;

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
        private Mock<IAuthorizedUser> authorizedUserMock;

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
            authorizedUserMock = new Mock<IAuthorizedUser>();

            webInterviewAllowService = new WebInterviewAllowService(transactionManagerProvider, 
                plainTransactionManagerProvider,
                interviewSummaryRepoMock.Object, 
                webInterviewConfigProvider,
                authorizedUserMock.Object,
                new EventBusSettings());
        }

        private void Act()
        {
            webInterviewAllowService.CheckWebInterviewAccessPermissions(interviewId.ToString());
        }

        private void ArrangeTest(
            UserRoles? loggedInUserRole = null, 
            InterviewStatus? interviewStatus = InterviewStatus.InterviewerAssigned, 
            bool webInterviewEnabled = true, 
            Guid? loggedInUserId = null, 
            Guid? responsibleId = null,
            Guid? teamLeadId = null)
        {
            if (loggedInUserId.HasValue && loggedInUserRole.HasValue)
            {
                this.authorizedUserMock.Setup(x => x.Id).Returns(loggedInUserId.Value);
                this.authorizedUserMock.Setup(x => x.IsAuthenticated).Returns(true);
                switch (loggedInUserRole)
                {
                    case UserRoles.Headquarter:
                        this.authorizedUserMock.Setup(x => x.IsHeadquarter).Returns(true);
                        break;
                    case UserRoles.Supervisor:
                        this.authorizedUserMock.Setup(x => x.IsSupervisor).Returns(true);
                        break;
                    case UserRoles.Interviewer:
                        this.authorizedUserMock.Setup(x => x.IsInterviewer).Returns(true);
                        break;
                }
            }

            if (interviewStatus.HasValue)
            {                
                interview = Create.Entity.InterviewSummary(responsibleId: responsibleId ?? Guid.NewGuid(), 
                                                        status: interviewStatus.Value,
                                                        teamLeadId: teamLeadId);
                
                webInterviewConfig.Started = webInterviewEnabled;
                
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
            catch(InterviewAccessException)
            {
                return false;
            }
            return true;
        }

        [Test]
        public void should_not_allow_user_that_is_not_responsible()
        {
            ArrangeTest(UserRoles.Interviewer, InterviewStatus.InterviewerAssigned, 
                webInterviewEnabled: false, loggedInUserId: Id.g1, responsibleId:Id.g2);

            Assert.Throws<InterviewAccessException>(Act);
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
                webInterviewEnabled: false, loggedInUserId: Id.g1, responsibleId: Id.g1);
            Act();
        }

        [TestCase(UserRoles.Interviewer, ExpectedResult = true)]
        [TestCase(UserRoles.Headquarter, ExpectedResult = false)]
        public bool should_allow_access_for_interviewerOnly_when_webInterview_disabled(UserRoles userRole)
        {
            ArrangeTest(userRole, webInterviewEnabled: false, loggedInUserId: Id.g1, responsibleId: Id.g1);

            try
            {
                Act();
            }
            catch(InterviewAccessException)
            {
                return false;
            }
            return true;
        }


        [Test]
        public void should_not_allow_access_if_interview_is_not_exists()
        {
            ArrangeTest(interviewStatus: null);

            Assert.Throws<InterviewAccessException>(Act);
        }

        [Test]
        public void should_not_allow_access_for_deleted_interview()
        {
            ArrangeTest(interviewStatus: InterviewStatus.Deleted);

            Assert.Throws<InterviewAccessException>(Act);
        }

        [Test]
        public void should_not_allow_access_web_interview_for_loggedin_user_that_is_not_responsible_with_valid_reason()
        {
            ArrangeTest(interviewStatus: InterviewStatus.InterviewerAssigned, webInterviewEnabled: false, 
                loggedInUserRole: UserRoles.Administrator,
                loggedInUserId: Id.gA,
                responsibleId: Id.g1);

            var exception = Assert.Throws<InterviewAccessException>(Act);
            Assert.That(exception.Reason, Is.EqualTo(InterviewAccessExceptionReason.UserNotAuthorised));
        }

        [Test]
        public void should_not_allow_access_web_interview_for_anonymous_user_if_it_is_stopped()
        {
            ArrangeTest(interviewStatus: InterviewStatus.InterviewerAssigned, webInterviewEnabled: false, 
                responsibleId: Id.g1);

            var exception = Assert.Throws<InterviewAccessException>(Act);
            Assert.That(exception.Reason, Is.EqualTo(InterviewAccessExceptionReason.InterviewExpired));
        }
    }
}
