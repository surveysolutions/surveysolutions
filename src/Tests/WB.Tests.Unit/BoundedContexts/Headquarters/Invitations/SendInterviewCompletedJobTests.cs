#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.Infrastructure.Domain;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Invitations
{
    [TestFixture]
    public class SendInterviewCompletedJobTests
    {
        private Mock<ICompletedEmailsQueue> queue = null!;
        private Mock<IInterviewCompletedEmailBuilder> builder = null!;
        private Mock<IEmailService> sender = null!;
        private ScopedExecutor<ICompletedEmailsQueue> queueScope = null!;
        private ScopedExecutor<IInterviewCompletedEmailBuilder> preparationScope = null!;
        private SendInterviewCompletedJob job = null!;
        private readonly PreparedInterviewCompletedEmail message = new()
        {
            Address = "respondent@example.com", Subject = "Completed", Html = "html", Text = "text"
        };

        [SetUp]
        public void SetUp()
        {
            queue = new Mock<ICompletedEmailsQueue>();
            builder = new Mock<IInterviewCompletedEmailBuilder>();
            sender = new Mock<IEmailService>();
            sender.Setup(s => s.IsConfigured()).Returns(true);
            sender.Setup(s => s.GetSenderInfo()).Returns(Mock.Of<ISenderInformation>());
            builder.Setup(b => b.PrepareAsync(It.IsAny<Guid>(), It.IsAny<ISenderInformation>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(message);
            queueScope = new ScopedExecutor<ICompletedEmailsQueue>(queue.Object);
            preparationScope = new ScopedExecutor<IInterviewCompletedEmailBuilder>(builder.Object);
            job = new SendInterviewCompletedJob(NullLogger<SendInterviewCompletedJob>.Instance,
                sender.Object, queueScope, preparationScope);
        }

        [Test]
        public async Task should_release_preparation_scope_before_sending_and_acknowledge_separately()
        {
            var id = Guid.NewGuid();
            queue.Setup(q => q.GetInterviewIdsForSend(100)).Returns(new List<Guid> { id });
            sender.Setup(s => s.SendEmailAsync(message.Address, message.Subject, message.Html, message.Text, message.Attachments))
                .Returns(async () =>
                {
                    await Task.Yield();
                    Assert.That(preparationScope.Active, Is.False);
                    Assert.That(queueScope.Active, Is.False);
                    Assert.That(preparationScope.Completed, Is.EqualTo(1));
                    return "message-id";
                });
            queue.Setup(q => q.Remove(id)).Callback(() => Assert.That(queueScope.Active, Is.True));

            await job.Execute(Mock.Of<IJobExecutionContext>());

            queue.Verify(q => q.Remove(id), Times.Once);
            Assert.That(queueScope.Completed, Is.EqualTo(2)); // fetch and acknowledge
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task should_count_failure_once_without_undoing_prior_acknowledgements(bool failDuringSend)
        {
            var first = Guid.NewGuid();
            var second = Guid.NewGuid();
            queue.Setup(q => q.GetInterviewIdsForSend(100)).Returns(new List<Guid> { first, second });
            var sends = 0;
            sender.Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<EmailAttachment>>()))
                .Returns(() => ++sends == 2 && failDuringSend
                    ? Task.FromException<string>(new InvalidOperationException("Send failed"))
                    : Task.FromResult("sent"));
            if (!failDuringSend)
                builder.Setup(b => b.PrepareAsync(second, It.IsAny<ISenderInformation>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new InvalidOperationException("Preparation failed"));
            queue.Setup(q => q.MarkAsFailedToSend(second)).Callback(() =>
            {
                Assert.That(preparationScope.Active, Is.False);
                Assert.That(queueScope.Active, Is.True);
                queue.Verify(q => q.Remove(first), Times.Once);
            });

            await job.Execute(Mock.Of<IJobExecutionContext>());

            queue.Verify(q => q.MarkAsFailedToSend(second), Times.Once);
            queue.Verify(q => q.MarkAsFailedToSend(first), Times.Never);
            queue.Verify(q => q.Remove(second), Times.Never);
        }

        [Test]
        public async Task should_fetch_only_one_bounded_batch_even_when_more_work_exists()
        {
            var ids = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid()).ToList();
            queue.Setup(q => q.GetInterviewIdsForSend(100)).Returns(ids);
            builder.Setup(b => b.PrepareAsync(It.IsAny<Guid>(), It.IsAny<ISenderInformation>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PreparedInterviewCompletedEmail?)null);

            await job.Execute(Mock.Of<IJobExecutionContext>());

            queue.Verify(q => q.GetInterviewIdsForSend(100), Times.Once);
            queue.Verify(q => q.Remove(It.IsAny<Guid>()), Times.Exactly(100));
            sender.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<List<EmailAttachment>>()), Times.Never);
        }

        [Test]
        public async Task cancellation_during_preparation_should_not_send_or_count_failure()
        {
            using var cts = new CancellationTokenSource();
            var id = Guid.NewGuid();
            queue.Setup(q => q.GetInterviewIdsForSend(100)).Returns(new List<Guid> { id });
            builder.Setup(b => b.PrepareAsync(id, It.IsAny<ISenderInformation>(), cts.Token))
                .Returns(() => { cts.Cancel(); return Task.FromResult<PreparedInterviewCompletedEmail?>(message); });

            await job.Execute(Mock.Of<IJobExecutionContext>(c => c.CancellationToken == cts.Token));

            queue.Verify(q => q.MarkAsFailedToSend(It.IsAny<Guid>()), Times.Never);
            queue.Verify(q => q.Remove(It.IsAny<Guid>()), Times.Never);
            sender.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<List<EmailAttachment>>()), Times.Never);
        }

        [Test]
        public async Task cancellation_during_successful_send_should_still_acknowledge_it()
        {
            using var cts = new CancellationTokenSource();
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            queue.Setup(q => q.GetInterviewIdsForSend(100)).Returns(ids);
            sender.Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<EmailAttachment>>()))
                .Returns(() => { cts.Cancel(); return Task.FromResult("sent"); });

            await job.Execute(Mock.Of<IJobExecutionContext>(c => c.CancellationToken == cts.Token));

            queue.Verify(q => q.Remove(ids[0]), Times.Once);
            queue.Verify(q => q.Remove(ids[1]), Times.Never);
            queue.Verify(q => q.MarkAsFailedToSend(It.IsAny<Guid>()), Times.Never);
        }

        private sealed class ScopedExecutor<TService> : IInScopeExecutor<TService>
        {
            private readonly TService service;
            public bool Active { get; private set; }
            public int Completed { get; private set; }
            public ScopedExecutor(TService service) => this.service = service;
            public void Execute(Action<TService> action, string workspace = null!) =>
                Execute(s => { action(s); return true; }, workspace);
            public TResult Execute<TResult>(Func<TService, TResult> action, string workspace = null!)
            {
                Active = true;
                try { var result = action(service); Completed++; return result; }
                finally { Active = false; }
            }
            public async Task ExecuteAsync(Func<TService, Task> action, string workspace = null!) =>
                await ExecuteAsync(async s => { await action(s); return true; }, workspace);
            public async Task<TResult> ExecuteAsync<TResult>(Func<TService, Task<TResult>> action, string workspace = null!)
            {
                Active = true;
                try { var result = await action(service); Completed++; return result; }
                finally { Active = false; }
            }
        }
    }
}

