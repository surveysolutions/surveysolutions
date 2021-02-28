using System;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.WebInterview.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.Services.Impl;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.WebInterview
{
    [TestFixture]
    [TestOf(typeof(WebInterviewAllowService))]
    public class WebInterviewAllowServiceTests
    {
        private WebInterviewAllowService webInterviewAllowService;
        private Guid interviewId = Guid.NewGuid();
        private IWebInterviewConfigProvider webInterviewConfigProvider;
        private WebInterviewConfig webInterviewConfig;
        private Mock<IAuthorizedUser> authorizedUserMock;
        private Mock<IAggregateRootPrototypeService> prototypeService;
        private Mock<IStatefulInterviewRepository> statefulInterviewRepo;
        private StatefulInterview interview;


        [SetUp]
        public void Setup()
        {
            statefulInterviewRepo = new Mock<IStatefulInterviewRepository>();
            webInterviewConfig = new WebInterviewConfig();
            webInterviewConfigProvider = Mock.Of<IWebInterviewConfigProvider>(tmp => tmp.Get(It.IsAny<QuestionnaireIdentity>()) == webInterviewConfig);
            authorizedUserMock = new Mock<IAuthorizedUser>();
            prototypeService = new Mock<IAggregateRootPrototypeService>();

            var interviewAllowService = new WebInterviewAllowService(
                statefulInterviewRepo.Object,
                webInterviewConfigProvider,
                authorizedUserMock.Object, 
                prototypeService.Object);
            webInterviewAllowService = interviewAllowService;
        }

        private void Act()
        {
            webInterviewAllowService.CheckWebInterviewAccessPermissions(interviewId.FormatGuid());
        }

        private void ArrangeTest(
            UserRoles? loggedInUserRole = null, 
            InterviewStatus? interviewStatus = InterviewStatus.WebInterview, 
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
                interview = Create.AggregateRoot.StatefulInterview(interviewId, userId: responsibleId, supervisorId: teamLeadId);
                var @event = Create.PublishedEvent.SynchronizationMetadataApplied(userId: responsibleId.FormatGuid(),
                    status: interviewStatus.Value);
                interview.Apply(@event.Payload);
                this.statefulInterviewRepo.Setup(r => r.Get(interviewId.FormatGuid())).Returns(interview);
                
                webInterviewConfig.Started = webInterviewEnabled;
            }
        }

        [TestCase(InterviewStatus.InterviewerAssigned, ExpectedResult = false)]
        [TestCase(InterviewStatus.Restarted, ExpectedResult = false)]
        [TestCase(InterviewStatus.Completed, ExpectedResult = false)]
        [TestCase(InterviewStatus.ApprovedByHeadquarters, ExpectedResult = false)]
        [TestCase(InterviewStatus.RejectedByHeadquarters, ExpectedResult = false)]
        [TestCase(InterviewStatus.RejectedBySupervisor, ExpectedResult = false)]
        [TestCase(InterviewStatus.WebInterview, ExpectedResult = true)]
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

        [Test]
        public void should_allow_administrator_to_access_ignored_web_interview()
        {
            this.authorizedUserMock.Setup(x => x.Id).Returns(Id.g1);
            this.authorizedUserMock.Setup(x => x.IsAuthenticated).Returns(true);
            this.authorizedUserMock.Setup(x => x.IsHeadquarter).Returns(true);

            this.prototypeService.Setup(p => p.GetPrototypeType(It.IsAny<Guid>())).Returns(PrototypeType.Temporary);

            // Act
            Assert.DoesNotThrow(Act);
        }
    }
}
