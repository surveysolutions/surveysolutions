using System;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Shared.Web.Services;

namespace WB.Tests.Unit.Designer.Code
{
    [TestFixture]
    [TestOf(typeof(MailNotifier))]
    public class MailNotifierTests
    {
        private static MailNotifier CreateNotifier(IEmailSender emailSender, IViewRenderService renderService = null)
        {
            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            urlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(Mock.Of<IUrlHelper>());

            if (renderService == null)
            {
                var renderServiceMock = new Mock<IViewRenderService>();
                renderServiceMock.Setup(r => r.RenderToStringAsync(It.IsAny<string>(), It.IsAny<object>(),
                        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RouteData>()))
                    .ReturnsAsync("body");
                renderService = renderServiceMock.Object;
            }

            return new MailNotifier(emailSender,
                renderService,
                Mock.Of<IHttpContextAccessor>(a => a.HttpContext == new DefaultHttpContext()),
                urlHelperFactory.Object,
                Mock.Of<ILogger<MailNotifier>>());
        }

        private static void Notify(MailNotifier notifier, string email = "target@example.com") =>
            notifier.NotifyTargetPersonAboutShareChange(ShareChangeType.Share, email, "user",
                Guid.NewGuid().ToString("N"), "title", ShareType.Edit, "actor@example.com");

        [Test]
        public void when_notifying_should_not_send_email_until_pending_notifications_are_sent()
        {
            var emailSender = new Mock<IEmailSender>();
            var notifier = CreateNotifier(emailSender.Object);

            Notify(notifier);
            notifier.NotifyOwnerAboutShareChange(ShareChangeType.Share, "owner@example.com", "owner",
                Guid.NewGuid().ToString("N"), "title", ShareType.Edit, "actor@example.com", "target@example.com");

            emailSender.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public async Task when_sending_pending_notifications_should_send_each_queued_email_once()
        {
            var emailSender = new Mock<IEmailSender>();
            var notifier = CreateNotifier(emailSender.Object);

            Notify(notifier);
            notifier.NotifyOwnerAboutShareChange(ShareChangeType.Share, "owner@example.com", "owner",
                Guid.NewGuid().ToString("N"), "title", ShareType.Edit, "actor@example.com", "target@example.com");

            await notifier.SendPendingNotificationsAsync();
            await notifier.SendPendingNotificationsAsync();

            emailSender.Verify(s => s.SendEmailAsync("target@example.com", It.IsAny<string>(), "body"), Times.Once);
            emailSender.Verify(s => s.SendEmailAsync("owner@example.com", It.IsAny<string>(), "body"), Times.Once);
        }

        [Test]
        public async Task when_pending_notifications_discarded_should_not_send_email()
        {
            var emailSender = new Mock<IEmailSender>();
            var notifier = CreateNotifier(emailSender.Object);

            Notify(notifier);
            notifier.DiscardPendingNotifications();
            await notifier.SendPendingNotificationsAsync();

            emailSender.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public void when_sending_one_notification_fails_should_continue_with_others_and_not_throw()
        {
            var emailSender = new Mock<IEmailSender>();
            emailSender.Setup(s => s.SendEmailAsync("first@example.com", It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("smtp failure"));
            var notifier = CreateNotifier(emailSender.Object);

            Notify(notifier, "first@example.com");
            Notify(notifier, "second@example.com");

            Assert.DoesNotThrowAsync(() => notifier.SendPendingNotificationsAsync());

            emailSender.Verify(s => s.SendEmailAsync("second@example.com", It.IsAny<string>(), "body"), Times.Once);
        }
    }
}
