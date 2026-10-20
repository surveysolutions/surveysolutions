using System;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NHibernate;
using NUnit.Framework;
using WB.Core.Infrastructure.Modularity;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Tests.Unit.Infrastructure
{
    [TestFixture]
    public class UnitOfWorkConnectionLifecycleTests
    {
        [Test]
        public void concurrent_first_access_should_open_only_one_session()
        {
            var session = new Mock<ISession>();
            session.Setup(s => s.BeginTransaction(IsolationLevel.ReadCommitted)).Returns(Mock.Of<ITransaction>());
            var factory = new Mock<ISessionFactory>();
            var opens = 0;
            factory.Setup(f => f.OpenSession()).Returns(() =>
            {
                Interlocked.Increment(ref opens);
                Thread.Sleep(20);
                return session.Object;
            });
            using var container = CreateContainer(factory.Object);
            using var scope = container.BeginLifetimeScope();
            using var uow = new UnitOfWork(NullLogger<UnitOfWork>.Instance, Mock.Of<IWorkspaceContextAccessor>(), scope);

            Parallel.For(0, 32, _ => Assert.That(uow.Session, Is.SameAs(session.Object)));

            Assert.That(opens, Is.EqualTo(1));
        }

        [Test]
        public void failed_begin_transaction_should_dispose_session_and_allow_fresh_attempt()
        {
            var failure = new InvalidOperationException("Begin failed");
            var failed = new Mock<ISession>();
            failed.Setup(s => s.BeginTransaction(IsolationLevel.ReadCommitted)).Throws(failure);
            var next = new Mock<ISession>();
            next.Setup(s => s.BeginTransaction(IsolationLevel.ReadCommitted)).Returns(Mock.Of<ITransaction>());
            var factory = new Mock<ISessionFactory>();
            factory.SetupSequence(f => f.OpenSession()).Returns(failed.Object).Returns(next.Object);
            using var container = CreateContainer(factory.Object);
            using var scope = container.BeginLifetimeScope();
            using var uow = new UnitOfWork(NullLogger<UnitOfWork>.Instance, Mock.Of<IWorkspaceContextAccessor>(), scope);

            Assert.That(Assert.Throws<InvalidOperationException>(() => { _ = uow.Session; }), Is.SameAs(failure));
            failed.Verify(s => s.Dispose(), Times.Once);
            Assert.That(uow.Session, Is.SameAs(next.Object));
        }

        [TestCase("commit")]
        [TestCase("transaction-dispose")]
        [TestCase("session-dispose")]
        public void should_stop_committing_but_clean_up_all_sessions_after_failure(string failureStage)
        {
            var error = new InvalidOperationException("First workspace failed");
            var commits = 0;
            var rollbacks = 0;
            var transactionDisposals = 0;
            var sessionDisposals = 0;
            Mock<ISession> CreateSession()
            {
                var tx = new Mock<ITransaction>();
                tx.SetupGet(t => t.IsActive).Returns(true);
                tx.Setup(t => t.Commit()).Callback(() =>
                {
                    commits++;
                    if (failureStage == "commit") throw error;
                });
                tx.Setup(t => t.Rollback()).Callback(() => rollbacks++);
                tx.Setup(t => t.Dispose()).Callback(() =>
                {
                    if (++transactionDisposals == 1 && failureStage == "transaction-dispose") throw error;
                });
                var session = new Mock<ISession>();
                session.Setup(s => s.BeginTransaction(IsolationLevel.ReadCommitted)).Returns(tx.Object);
                session.Setup(s => s.Dispose()).Callback(() =>
                {
                    if (++sessionDisposals == 1 && failureStage == "session-dispose") throw error;
                });
                return session;
            }
            var factory = new Mock<ISessionFactory>();
            factory.SetupSequence(f => f.OpenSession()).Returns(CreateSession().Object).Returns(CreateSession().Object);
            using var container = CreateContainer(factory.Object);
            using var scope = container.BeginLifetimeScope();
            var workspace = new WorkspaceContext("one", "One");
            var accessor = new Mock<IWorkspaceContextAccessor>();
            accessor.Setup(a => a.CurrentWorkspace()).Returns(() => workspace);
            var uow = new UnitOfWork(NullLogger<UnitOfWork>.Instance, accessor.Object, scope);
            _ = uow.Session;
            workspace = new WorkspaceContext("two", "Two");
            _ = uow.Session;
            uow.AcceptChanges();

            Assert.That(Assert.Throws<InvalidOperationException>(() => uow.Dispose()), Is.SameAs(error));
            Assert.That(commits, Is.EqualTo(1));
            Assert.That(rollbacks, Is.EqualTo(1));
            Assert.That(transactionDisposals, Is.EqualTo(2));
            Assert.That(sessionDisposals, Is.EqualTo(2));
            Assert.DoesNotThrow(() => uow.Dispose());
            Assert.That(sessionDisposals, Is.EqualTo(2));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void should_evict_failed_factory_without_removing_a_newer_replacement(bool replaceDuringFailure)
        {
            var key = "review-" + Guid.NewGuid().ToString("N");
            var workspace = new WorkspaceContext(key, key);
            var context = new Mock<IModuleContext>();
            context.Setup(c => c.Resolve<IWorkspaceContextAccessor>())
                .Returns(Mock.Of<IWorkspaceContextAccessor>(a => a.CurrentWorkspace() == workspace));
            var cache = (ConcurrentDictionary<string, Lazy<ISessionFactory>>)typeof(HqSessionFactoryFactory)
                .GetField("sessionFactories", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            var healthy = Mock.Of<ISessionFactory>();
            var replacement = new Lazy<ISessionFactory>(() => healthy);
            var failure = new InvalidOperationException("Transient failure");
            cache[key] = new Lazy<ISessionFactory>(() =>
            {
                if (replaceDuringFailure) cache[key] = replacement;
                throw failure;
            });
            try
            {
                var factories = new HqSessionFactoryFactory(new UnitOfWorkConnectionSettings());
                Assert.That(Assert.Throws<InvalidOperationException>(() => factories.SessionFactoryBinder(context.Object)), Is.SameAs(failure));
                if (replaceDuringFailure)
                    Assert.That(cache[key], Is.SameAs(replacement));
                else
                    Assert.That(cache.ContainsKey(key), Is.False);
                cache.TryAdd(key, replacement);
                Assert.That(factories.SessionFactoryBinder(context.Object), Is.SameAs(healthy));
            }
            finally
            {
                cache.TryRemove(key, out _);
            }
        }

        private static IContainer CreateContainer(ISessionFactory factory)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(factory).As<ISessionFactory>().ExternallyOwned();
            return builder.Build();
        }
    }
}

