using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.UI.Designer.CommonWeb;

namespace WB.Tests.Unit.Designer.CommonWeb
{
    [TestFixture]
    [TestOf(typeof(MailSender))]
    internal class MailSenderTests
    {
        [Test]
        public async Task SendEmailAsync_when_timeout_is_negative_should_fall_back_to_default_timeout()
        {
            var pickupFolder = Path.Combine(Path.GetTempPath(), "MailSenderTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(pickupFolder);

            var settings = Options.Create(new MailSettings
            {
                Host = "localhost",
                Port = 25,
                From = "from@example.com",
                UsePickupFolder = true,
                PickupFolder = pickupFolder,
                Timeout = -1
            });
            var environment = new Mock<IWebHostEnvironment>();
            var mailSender = new MailSender(settings, environment.Object);

            await mailSender.SendEmailAsync("to@example.com", "subject", "<p>body</p>");

            Assert.That(Directory.GetFiles(pickupFolder), Has.Length.EqualTo(1));
        }
    }
}
