using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Invitations
{
    [TestFixture]
    public class EmailServiceScopeTests
    {
        [Test]
        public void provider_configuration_should_only_read_database_inside_short_scope()
        {
            var active = false;
            var settings = new EmailProviderSettings
            {
                Provider = EmailProvider.Smtp, SmtpHost = "localhost", SmtpAuthentication = false,
                SenderAddress = "sender@example.com"
            };
            var storage = new Mock<IPlainKeyValueStorage<EmailProviderSettings>>();
            storage.Setup(s => s.GetById(AppSetting.EmailProviderSettings)).Returns(() =>
            {
                Assert.That(active, Is.True);
                return settings;
            });
            var executor = new Mock<IInScopeExecutor<IPlainKeyValueStorage<EmailProviderSettings>>>();
            executor.Setup(e => e.Execute(It.IsAny<Func<IPlainKeyValueStorage<EmailProviderSettings>, EmailProviderSettings>>(), null))
                .Returns((Func<IPlainKeyValueStorage<EmailProviderSettings>, EmailProviderSettings> action, string workspace) =>
                {
                    active = true;
                    try { return action(storage.Object); }
                    finally { active = false; }
                });
            var service = new EmailService(executor.Object, Mock.Of<ISerializer>());

            Assert.That(service.IsConfigured(), Is.True);
            Assert.That(active, Is.False);
            storage.Verify(s => s.GetById(AppSetting.EmailProviderSettings), Times.Once);

            Assert.That(service.GetSenderInfo().SenderAddress, Is.EqualTo("sender@example.com"));
            storage.Verify(s => s.GetById(AppSetting.EmailProviderSettings), Times.Exactly(2));

            // Configuration changes must be seen on the next operation, not cached in the sender.
            settings = new EmailProviderSettings { Provider = EmailProvider.None };
            Assert.That(service.IsConfigured(), Is.False);
            Assert.That(active, Is.False);
            Assert.Throws<Exception>(() => service.SendEmailAsync("recipient@example.com", "subject", "html", "text", null));
            Assert.That(active, Is.False);
            storage.Verify(s => s.GetById(AppSetting.EmailProviderSettings), Times.Exactly(4));
        }
    }
}

