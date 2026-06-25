using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using reCAPTCHA.AspNetCore;
using WB.Enumerator.Native.WebInterview;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Assignment;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers;
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Services;

namespace WB.Tests.Unit.Applications.Headquarters
{
    [TestFixture]
    [TestOf(typeof(WebInterviewController))]
    public class WebInterviewControllerTests
    {
        [TestCase(1, "11111111111111111111111111111111")]
        [TestCase(100, "11111111111111111111111111111111")]
        public void when_start_interview_should_save_info_in_session(int quantity, string interviewId)
        {
            var controller = CreateController(quantity, interviewId);

            controller.Start("invitation");

            Assert.That(controller.HttpContext.Session.Keys.Count(), Is.EqualTo(1));
            var sessionKey = controller.HttpContext.Session.Keys.Single();
            Assert.That(sessionKey.StartsWith("WebInterview-"), Is.True);
            Assert.That(controller.HttpContext.Session.Get<bool>(sessionKey), Is.EqualTo(true));
        }

        [TestCase(1, "11111111111111111111111111111111")]
        [TestCase(100, "11111111111111111111111111111111")]
        public void when_resume_interview_should_save_info_in_session(int quantity, string interviewId)
        {
            var controller = CreateController(quantity, interviewId);

            controller.Resume(interviewId, "returnUrl");

            Assert.That(controller.HttpContext.Session.Get<bool>($"WebInterview-{interviewId}"), Is.EqualTo(true));
        }

        [Test]
        public void when_starting_closed_assignment_should_throw_expired_for_stale_cached_invitation()
        {
            var controller = CreateController(100, null,
                invitationAssignmentStatus: AssignmentStatus.Open,
                currentAssignmentStatus: AssignmentStatus.Closed);

            var exception = Assert.Throws<InterviewAccessException>(() => controller.Start("invitation"));

            Assert.That(exception?.Reason, Is.EqualTo(InterviewAccessExceptionReason.InterviewExpired));
        }

        [Test]
        public void when_posting_start_for_closed_assignment_should_throw_expired_for_stale_cached_invitation()
        {
            var controller = CreateController(100, null,
                invitationAssignmentStatus: AssignmentStatus.Open,
                currentAssignmentStatus: AssignmentStatus.Closed);

            var exception = Assert.ThrowsAsync<InterviewAccessException>(() => controller.StartPost("invitation", string.Empty));

            Assert.That(exception?.Reason, Is.EqualTo(InterviewAccessExceptionReason.InterviewExpired));
        }

        private WebInterviewController CreateController(int quantity, string interviewId,
            AssignmentStatus invitationAssignmentStatus = AssignmentStatus.Open,
            AssignmentStatus currentAssignmentStatus = AssignmentStatus.Open)
        {
            var invitationAssignment = Mock.Of<Assignment>(a =>
                a.Id == 3
                && a.WebMode == true
                && a.Quantity == quantity
                && a.Status == invitationAssignmentStatus);

            var currentAssignment = Mock.Of<Assignment>(a =>
                a.Id == 3
                && a.WebMode == true
                && a.Quantity == quantity
                && a.Status == currentAssignmentStatus);

            var invitation = Mock.Of<Invitation>(i =>
                i.InterviewId == interviewId
                && i.AssignmentId == 3
                && i.Assignment == invitationAssignment
                && i.IsWithAssignmentResolvedByPassword() == true

                && i.Interview == Mock.Of<InterviewSummary>(s =>
                    s.GetInterviewProperties() == new InterviewProperties
                    {
                        Mode = InterviewMode.CAWI, 
                        Status = InterviewStatus.InterviewerAssigned
                    }
                && s.Status == InterviewStatus.InterviewerAssigned));

            var o = (object)invitation;

            var memoryCache = new Mock<IMemoryCache>();
            memoryCache.Setup(m => m.TryGetValue(It.IsAny<object>(), out o))
                .Returns(true);

            var config = Mock.Of<WebInterviewConfig>(c => c.Started == true
                && c.SingleResponse == true); 
            var webInterviewConfigProvider = Mock.Of<IWebInterviewConfigProvider>(c =>
                c.Get(It.IsAny<QuestionnaireIdentity>()) == config);
            
            var user = Mock.Of<UserViewLite>();
            var usersRepository = Mock.Of<IUserViewFactory>(u => u.GetUser(It.IsAny<Guid>()) == user);

            var assignmentsService = Mock.Of<IAssignmentsService>(a => a.GetAssignment(invitation.AssignmentId) == currentAssignment);
            var invitationService = Mock.Of<IInvitationService>(i =>
                i.GetInvitationByTokenAndPassword(It.IsAny<string>(), It.IsAny<string>()) == invitation);
            
            var statefulInterviewRepository = Mock.Of<IStatefulInterviewRepository>(r => r.Get(interviewId) == Mock.Of<IStatefulInterview>());

            var controller = new WebInterviewController(
                Mock.Of<ICommandService>(),
                webInterviewConfigProvider,
                statefulInterviewRepository,
                usersRepository,
                Mock.Of<IInterviewUniqueKeyGenerator>(),
                Mock.Of<ICaptchaProvider>(),
                assignmentsService,
                invitationService,
                Mock.Of<INativeReadSideStorage<InterviewSummary>>(),
                Mock.Of<IInvitationMailingService>(),
                Mock.Of<IPlainKeyValueStorage<EmailProviderSettings>>(),
                Mock.Of<IOptions<RecaptchaSettings>>(),
                Mock.Of<IOptions<CaptchaConfig>>(),
                Mock.Of<IServiceLocator>(),
                Mock.Of<IQuestionnaireStorage>(),
                Mock.Of<IInScopeExecutor>(),
                memoryCache.Object,
                calendarEventService: Mock.Of<ICalendarEventService>(),
                webInterviewConfigProvider: Mock.Of<IWebInterviewConfigProvider>() ,
                webInterviewLinkProvider: Mock.Of<IWebInterviewLinkProvider>());
            var request = new Mock<HttpRequest>();
            request.SetupGet(r => r.Cookies).Returns(Mock.Of<IRequestCookieCollection>());
            request.SetupGet(r => r.Form).Returns(new FormCollection(new Dictionary<string, StringValues>()));

            var response = new Mock<HttpResponse>();
            response.SetupGet(r => r.Cookies).Returns(Mock.Of<IResponseCookies>());

            controller.ControllerContext.HttpContext = Mock.Of<HttpContext>(c =>
                c.Session == new MockHttpSession()
                && c.Request == request.Object
                && c.Response == response.Object);
            if (interviewId != null)
            {
                Mock.Get(request.Object.Cookies)
                    .Setup(c => c[$"InterviewId-{invitationAssignment.Id}"])
                    .Returns(interviewId);
            }
            controller.Url = Mock.Of<IUrlHelper>(x => x.Action(It.IsAny<UrlActionContext>()) == "url");

            return controller;
        }
    }
}
