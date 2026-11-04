using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
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
        public async Task SendEmailAsync_when_smtp_server_stalls_should_cancel_send_and_close_connection()
        {
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var settings = Options.Create(new MailSettings
            {
                Host = "127.0.0.1",
                Port = ((IPEndPoint)listener.LocalEndpoint).Port,
                From = "from@example.com",
                Timeout = 1_000
            });
            var mailSender = new MailSender(settings, Mock.Of<IWebHostEnvironment>());

            var sendTask = mailSender.SendEmailAsync("to@example.com", "subject", "<p>body</p>");
            using var connection = await listener.AcceptTcpClientAsync().WaitAsync(TimeSpan.FromSeconds(10));

            Assert.CatchAsync<OperationCanceledException>(
                async () => await sendTask.WaitAsync(TimeSpan.FromSeconds(10)));
            var bytesRead = await connection.GetStream().ReadAsync(new byte[1]).AsTask()
                .WaitAsync(TimeSpan.FromSeconds(10));
            Assert.That(bytesRead, Is.Zero, "Cancellation should close the SMTP connection");
        }

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
