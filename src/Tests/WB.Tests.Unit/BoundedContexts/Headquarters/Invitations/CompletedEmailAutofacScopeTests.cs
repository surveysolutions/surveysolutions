using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NHibernate;
using Npgsql;
using NUnit.Framework;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.BoundedContexts.Headquarters.Workspaces.Impl;
using WB.Core.Infrastructure.Domain;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Invitations
{
    [TestFixture]
    public class CompletedEmailAutofacScopeTests
    {
        [TestCase("one", "none")]
        [TestCase("two", "none")]
        [TestCase("one", "preparation")]
        [TestCase("two", "preparation")]
        [TestCase("one", "send")]
        [TestCase("two", "send")]
        public async Task real_executor_should_release_transactions_and_inherit_workspace(string workspaceName, string failureStage)
        {
            var activeTransactions = 0;
            var commits = 0;
            var rollbacks = 0;
            var sessions = new List<Mock<ISession>>();
            var transactions = new List<Mock<ITransaction>>();
            var workspaces = new List<string>();
            var unitOfWorkIds = new List<long>();
            var transactionsDuringSend = new List<int>();
            var removed = new List<Guid>();
            var failed = new List<Guid>();
            var interviewId = Guid.NewGuid();
            var factory = new Mock<ISessionFactory>();
            factory.Setup(f => f.OpenSession()).Returns(() =>
            {
                var transaction = new Mock<ITransaction>();
                transaction.SetupGet(t => t.IsActive).Returns(() => true);
                transaction.Setup(t => t.Commit()).Callback(() => commits++);
                transaction.Setup(t => t.Rollback()).Callback(() => rollbacks++);
                var session = new Mock<ISession>();
                session.Setup(s => s.BeginTransaction(IsolationLevel.ReadCommitted)).Returns(() =>
                {
                    activeTransactions++;
                    return transaction.Object;
                });
                session.Setup(s => s.Dispose()).Callback(() => activeTransactions--);
                sessions.Add(session);
                transactions.Add(transaction);
                return session.Object;
            });

            void AccessDatabase(IUnitOfWork unitOfWork, IWorkspaceContextAccessor accessor)
            {
                workspaces.Add(accessor.CurrentWorkspace()?.Name);
                unitOfWorkIds.Add(((UnitOfWork)unitOfWork).Id);
                _ = unitOfWork.Session;
            }

            var registrations = new ContainerBuilder();
            registrations.RegisterInstance(factory.Object).As<ISessionFactory>().ExternallyOwned();
            registrations.RegisterInstance(NullLogger<UnitOfWork>.Instance)
                .As<Microsoft.Extensions.Logging.ILogger<UnitOfWork>>();
            registrations.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();
            registrations.RegisterType<WorkspaceContextHolder>().As<IWorkspaceContextHolder>().InstancePerLifetimeScope();
            registrations.RegisterType<WorkspaceContextAccessor>().As<IWorkspaceContextAccessor>();
            registrations.RegisterType<WorkspaceContextSetter>().As<IWorkspaceContextSetter>();
            registrations.RegisterInstance(Mock.Of<IWorkspacesCache>());
            registrations.RegisterGeneric(typeof(UnitOfWorkInScopeExecutor<>)).As(typeof(IInScopeExecutor<>));
            registrations.Register(context =>
            {
                var unitOfWork = context.Resolve<IUnitOfWork>();
                var accessor = context.Resolve<IWorkspaceContextAccessor>();
                var queue = new Mock<ICompletedEmailsQueue>();
                queue.Setup(q => q.GetInterviewIdsForSend(100)).Returns(() =>
                {
                    AccessDatabase(unitOfWork, accessor);
                    return new List<Guid> { interviewId };
                });
                queue.Setup(q => q.Remove(interviewId)).Callback(() =>
                {
                    AccessDatabase(unitOfWork, accessor);
                    removed.Add(interviewId);
                });
                queue.Setup(q => q.MarkAsFailedToSend(interviewId)).Callback(() =>
                {
                    AccessDatabase(unitOfWork, accessor);
                    failed.Add(interviewId);
                });
                return queue.Object;
            }).As<ICompletedEmailsQueue>();
            registrations.Register(context =>
            {
                var unitOfWork = context.Resolve<IUnitOfWork>();
                var accessor = context.Resolve<IWorkspaceContextAccessor>();
                var preparation = new Mock<IInterviewCompletedEmailBuilder>();
                preparation.Setup(p => p.PrepareAsync(interviewId, It.IsAny<ISenderInformation>(), It.IsAny<CancellationToken>()))
                    .Returns(async () =>
                    {
                        AccessDatabase(unitOfWork, accessor);
                        await Task.Yield();
                        if (failureStage == "preparation") throw new InvalidOperationException("Preparation failed");
                        return new PreparedInterviewCompletedEmail();
                    });
                return preparation.Object;
            }).As<IInterviewCompletedEmailBuilder>();

            using var container = registrations.Build();
            using var jobScope = container.BeginLifetimeScope();
            jobScope.Resolve<IWorkspaceContextSetter>().Set(new WorkspaceContext(workspaceName, workspaceName));
            var sender = new Mock<IEmailService>();
            sender.Setup(s => s.IsConfigured()).Returns(true);
            sender.Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<List<EmailAttachment>>()))
                .Returns(async () =>
                {
                    transactionsDuringSend.Add(activeTransactions);
                    await Task.Yield();
                    transactionsDuringSend.Add(activeTransactions);
                    if (failureStage == "send") throw new InvalidOperationException("Send failed");
                    return "sent";
                });
            var job = new SendInterviewCompletedJob(NullLogger<SendInterviewCompletedJob>.Instance, sender.Object,
                jobScope.Resolve<IInScopeExecutor<ICompletedEmailsQueue>>(),
                jobScope.Resolve<IInScopeExecutor<IInterviewCompletedEmailBuilder>>());

            await job.Execute(Mock.Of<IJobExecutionContext>());

            // The outer job scope is still alive: none of its lifetime can hide leaked child sessions.
            Assert.That(activeTransactions, Is.Zero);
            Assert.That(sessions.Count, Is.EqualTo(3)); // fetch, preparation, acknowledgement
            Assert.That(unitOfWorkIds.Distinct().Count(), Is.EqualTo(3));
            Assert.That(workspaces, Is.All.EqualTo(workspaceName));
            Assert.That(transactionsDuringSend.Count, Is.EqualTo(failureStage == "preparation" ? 0 : 2));
            Assert.That(transactionsDuringSend, Is.All.Zero);
            Assert.That(rollbacks, Is.EqualTo(failureStage == "preparation" ? 1 : 0));
            Assert.That(commits, Is.EqualTo(3 - rollbacks));
            Assert.That(removed.Count, Is.EqualTo(failureStage == "none" ? 1 : 0));
            Assert.That(failed.Count, Is.EqualTo(failureStage == "none" ? 0 : 1));
            foreach (var session in sessions) session.Verify(s => s.Dispose(), Times.Once);
            foreach (var transaction in transactions) transaction.Verify(t => t.Dispose(), Times.Once);
        }

        [Test]
        public void installed_npgsql_should_default_to_one_hundred_connections_per_pool()
        {
            var settings = new NpgsqlConnectionStringBuilder();
            Assert.That(settings.Pooling, Is.True);
            Assert.That(settings.MaxPoolSize, Is.EqualTo(100));
            Assert.That(settings.MinPoolSize, Is.Zero);
        }
    }
}
