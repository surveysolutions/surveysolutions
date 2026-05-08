using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using reCAPTCHA.AspNetCore;
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
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models.WebInterview;
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
        public async Task when_resume_post_and_captcha_is_not_filled_should_return_resume_view_with_captcha_error()
        {
            // Arrange
            var interviewId = "11111111111111111111111111111111";
            var questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 1);

            var interview = Mock.Of<IStatefulInterview>(i =>
                i.QuestionnaireIdentity == questionnaireIdentity);

            var statefulInterviewRepository = Mock.Of<IStatefulInterviewRepository>(r =>
                r.Get(interviewId) == interview);

            var webInterviewConfig = new WebInterviewConfig
            {
                Started = true,
                UseCaptcha = true,
                CustomMessages = new Dictionary<WebInterviewUserMessages, string>()
            };
            var configProvider = Mock.Of<IWebInterviewConfigProvider>(c =>
                c.Get(It.IsAny<QuestionnaireIdentity>()) == webInterviewConfig);

            var questionnaire = Mock.Of<IQuestionnaire>(q => q.Title == "Test Questionnaire");
            var questionnaireStorage = Mock.Of<IQuestionnaireStorage>(s =>
                s.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire);

            var captchaProvider = new Mock<ICaptchaProvider>();
            captchaProvider.Setup(c => c.IsCaptchaValid(It.IsAny<HttpRequest>()))
                .ReturnsAsync(false);

            var captchaConfig = Mock.Of<IOptions<CaptchaConfig>>(o =>
                o.Value == new CaptchaConfig { CaptchaType = CaptchaProviderType.Recaptcha });
            var recaptchaSettings = Mock.Of<IOptions<RecaptchaSettings>>(o =>
                o.Value == new RecaptchaSettings { SiteKey = "test-key" });

            var controller = new WebInterviewController(
                Mock.Of<ICommandService>(),
                configProvider,
                statefulInterviewRepository,
                Mock.Of<IUserViewFactory>(),
                Mock.Of<IInterviewUniqueKeyGenerator>(),
                captchaProvider.Object,
                Mock.Of<IAssignmentsService>(),
                Mock.Of<IInvitationService>(),
                Mock.Of<INativeReadSideStorage<InterviewSummary>>(),
                Mock.Of<IInvitationMailingService>(),
                Mock.Of<IPlainKeyValueStorage<EmailProviderSettings>>(),
                recaptchaSettings,
                captchaConfig,
                Mock.Of<IServiceLocator>(),
                questionnaireStorage,
                Mock.Of<IInScopeExecutor>(),
                Mock.Of<IMemoryCache>(),
                calendarEventService: Mock.Of<ICalendarEventService>(),
                webInterviewConfigProvider: Mock.Of<IWebInterviewConfigProvider>(),
                webInterviewLinkProvider: Mock.Of<IWebInterviewLinkProvider>());

            controller.ControllerContext.HttpContext = Mock.Of<HttpContext>(c =>
                c.Session == new MockHttpSession()
                && c.Request == Mock.Of<HttpRequest>(r =>
                    r.Cookies == Mock.Of<IRequestCookieCollection>()
                    && r.Form == Mock.Of<IFormCollection>())
                && c.Response == Mock.Of<HttpResponse>(r => r.Cookies == Mock.Of<IResponseCookies>()));

            controller.Url = Mock.Of<IUrlHelper>(x => x.Action(It.IsAny<UrlActionContext>()) == "url");
            controller.ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());

            // Act
            var result = await controller.ResumePost(interviewId, null, null);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null, "Expected ViewResult but got: " + result?.GetType().Name);
            Assert.That(viewResult!.ViewName, Is.EqualTo("Resume"));
            var model = viewResult.Model as ResumeWebInterview;
            Assert.That(model, Is.Not.Null, "Expected ResumeWebInterview model");
            Assert.That(model!.CaptchaErrors, Is.Not.Empty, "Expected captcha errors to be set");
        }

        private WebInterviewController CreateController(int quantity, string interviewId)
        {
            var assignment = Mock.Of<Assignment>(a =>
                a.Id == 3
                && a.WebMode == true
                && a.Quantity == quantity);

            var invitation = Mock.Of<Invitation>(i =>
                i.InterviewId == interviewId
                && i.Assignment == assignment
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
            
            var statefulInterviewRepository = Mock.Of<IStatefulInterviewRepository>(r => r.Get(interviewId) == Mock.Of<IStatefulInterview>());

            var controller = new WebInterviewController(
                Mock.Of<ICommandService>(),
                webInterviewConfigProvider,
                statefulInterviewRepository,
                usersRepository,
                Mock.Of<IInterviewUniqueKeyGenerator>(),
                Mock.Of<ICaptchaProvider>(),
                Mock.Of<IAssignmentsService>(),
                Mock.Of<IInvitationService>(),
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
            controller.ControllerContext.HttpContext = Mock.Of<HttpContext>(c => 
                c.Session == new MockHttpSession()
                && c.Request == Mock.Of<HttpRequest>(r => r.Cookies == Mock.Of<IRequestCookieCollection>())
                && c.Response == Mock.Of<HttpResponse>(r => r.Cookies == Mock.Of<IResponseCookies>()));
            if (interviewId != null)
            {
                Mock.Get(controller.ControllerContext.HttpContext.Request.Cookies)
                    .Setup(c => c[$"InterviewId-{assignment.Id}"])
                    .Returns(interviewId);
            }
            controller.Url = Mock.Of<IUrlHelper>(x => x.Action(It.IsAny<UrlActionContext>()) == "url");

            return controller;
        }
    }
}
