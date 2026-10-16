using System;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.UI.WebTester.Controllers;
using WB.UI.WebTester.Services;

namespace WB.Tests.Web.WebTester.Controllers
{
    [TestFixture]
    [TestOf(typeof(InterviewCommandsController))]
    internal class InterviewCommandsControllerTests
    {
        [Test]
        public void when_completing_interview_should_revoke_session_and_complete_eviction()
        {
            var interviewId = Guid.NewGuid();
            var questionnaireId = Guid.NewGuid();
            var session = Mock.Of<ISession>();
            var sessionFeature = new Mock<ISessionFeature>();
            sessionFeature.SetupGet(x => x.Session).Returns(session);

            var sessionService = new Mock<IWebTesterSessionService>();
            sessionService.Setup(x => x.GetQuestionnaireId(session, interviewId)).Returns(questionnaireId);
            var evictionNotifier = new Mock<IEvictionNotifier>();

            var httpContext = new DefaultHttpContext();
            httpContext.Features.Set(sessionFeature.Object);
            var controller = new InterviewCommandsController(
                Mock.Of<ICommandService>(),
                Mock.Of<IImageFileStorage>(),
                Mock.Of<IAudioFileStorage>(),
                Mock.Of<IQuestionnaireStorage>(),
                Mock.Of<IStatefulInterviewRepository>(),
                Mock.Of<IWebInterviewNotificationService>(),
                evictionNotifier.Object,
                sessionService.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };

            var result = controller.CompleteInterview(interviewId, null!);

            result.Should().BeOfType<OkResult>();
            sessionService.Verify(x => x.RevokeQuestionnaire(session, interviewId, questionnaireId), Times.Once);
            evictionNotifier.Verify(x => x.Complete(interviewId), Times.Once);
            evictionNotifier.Verify(x => x.Evict(interviewId), Times.Never);
        }
    }
}
