using System;
using System.Data;
using Autofac;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NHibernate;
using NUnit.Framework;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Tests.Web.Headquarters.Filters
{
    [TestFixture]
    public class UnitOfWorkCompletionTests
    {
        private IContainer container;
        private ILifetimeScope scope;
        private UnitOfWork unitOfWork;
        private Mock<ISessionFactory> sessionFactory;
        private Mock<IWorkspaceContextAccessor> workspace;
        private SessionState first;

        [SetUp]
        public void SetUp()
        {
            first = new SessionState();
            sessionFactory = new Mock<ISessionFactory>();
            sessionFactory.Setup(x => x.OpenSession()).Returns(first.Session.Object);
            workspace = new Mock<IWorkspaceContextAccessor>();
            var builder = new ContainerBuilder();
            builder.RegisterInstance(sessionFactory.Object).As<ISessionFactory>();
            container = builder.Build();
            scope = container.BeginLifetimeScope();
            unitOfWork = new UnitOfWork(NullLogger<UnitOfWork>.Instance, workspace.Object, scope);
        }

        [TearDown]
        public void TearDown()
        {
            unitOfWork.Dispose();
            scope.Dispose();
            container.Dispose();
        }

        [Test]
        public void should_commit_once_and_keep_session_open_for_read_only_rendering()
        {
            Assert.That(unitOfWork.Session, Is.SameAs(first.Session.Object));
            unitOfWork.AcceptChanges();

            unitOfWork.Complete();
            unitOfWork.Complete();

            first.WriteTransaction.Verify(x => x.Commit(), Times.Once);
            first.Session.Verify(x => x.Dispose(), Times.Never);
            first.Session.VerifySet(x => x.DefaultReadOnly = true, Times.Once);
            first.Session.VerifySet(x => x.FlushMode = FlushMode.Manual, Times.Once);
            first.Session.Verify(x => x.CreateSQLQuery("SET TRANSACTION READ ONLY"), Times.Once);
            Assert.That(unitOfWork.Session, Is.SameAs(first.Session.Object));
            Assert.Throws<InvalidOperationException>(() => unitOfWork.AcceptChanges());
            Assert.Throws<InvalidOperationException>(() => unitOfWork.DiscardChanges());

            unitOfWork.Dispose();

            first.WriteTransaction.Verify(x => x.Commit(), Times.Once);
            first.ReadTransaction.Verify(x => x.Commit(), Times.Never);
            first.ReadTransaction.Verify(x => x.Rollback(), Times.Once);
            first.Session.Verify(x => x.Dispose(), Times.Once);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void should_roll_back_unaccepted_or_discarded_changes(bool discard)
        {
            _ = unitOfWork.Session;
            if (discard)
            {
                unitOfWork.DiscardChanges();
                unitOfWork.AcceptChanges();
            }

            unitOfWork.Complete();

            first.WriteTransaction.Verify(x => x.Commit(), Times.Never);
            first.WriteTransaction.Verify(x => x.Rollback(), Times.Once);
        }

        [Test]
        public void should_not_retry_failed_commit_on_disposal()
        {
            _ = unitOfWork.Session;
            var failure = new InvalidOperationException("Commit failed");
            first.WriteTransaction.Setup(x => x.Commit()).Throws(failure);
            unitOfWork.AcceptChanges();

            Assert.That(Assert.Throws<InvalidOperationException>(() => unitOfWork.Complete()), Is.SameAs(failure));
            Assert.Throws<InvalidOperationException>(() => unitOfWork.Complete());
            Assert.Throws<InvalidOperationException>(() => { _ = unitOfWork.Session; });
            unitOfWork.Dispose();
            unitOfWork.Dispose();

            first.WriteTransaction.Verify(x => x.Commit(), Times.Once);
            first.WriteTransaction.Verify(x => x.Rollback(), Times.Once);
            first.WriteTransaction.Verify(x => x.Dispose(), Times.Once);
            first.Session.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void should_open_read_only_transaction_when_session_is_first_requested_after_completion()
        {
            unitOfWork.AcceptChanges();
            unitOfWork.Complete();
            sessionFactory.Verify(x => x.OpenSession(), Times.Never);

            _ = unitOfWork.Session;

            first.Session.Verify(x => x.CreateSQLQuery("SET TRANSACTION READ ONLY"), Times.Once);
            unitOfWork.Dispose();
            first.WriteTransaction.Verify(x => x.Commit(), Times.Never);
            first.WriteTransaction.Verify(x => x.Rollback(), Times.Once);
        }

        [Test]
        public void should_preserve_deferred_completion_for_background_callers()
        {
            _ = unitOfWork.Session;
            unitOfWork.AcceptChanges();
            first.WriteTransaction.Verify(x => x.Commit(), Times.Never);

            unitOfWork.Dispose();

            first.WriteTransaction.Verify(x => x.Commit(), Times.Once);
            first.Session.Verify(x => x.CreateSQLQuery(It.IsAny<string>()), Times.Never);
            first.Session.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public void should_clean_up_all_workspaces_even_if_commit_and_cleanup_fail()
        {
            _ = unitOfWork.Session;
            var second = new SessionState();
            sessionFactory.Setup(x => x.OpenSession()).Returns(second.Session.Object);
            workspace.Setup(x => x.CurrentWorkspace()).Returns(new WorkspaceContext("second", "Second"));
            _ = unitOfWork.Session;
            first.WriteTransaction.Setup(x => x.Commit()).Throws(new InvalidOperationException("Commit failed"));
            second.WriteTransaction.Setup(x => x.Commit()).Throws(new InvalidOperationException("Commit failed"));
            first.WriteTransaction.Setup(x => x.Rollback()).Throws(new InvalidOperationException("Rollback failed"));
            first.Session.Setup(x => x.Dispose()).Throws(new InvalidOperationException("Dispose failed"));
            unitOfWork.AcceptChanges();

            Assert.Throws<AggregateException>(() => unitOfWork.Dispose());

            first.WriteTransaction.Verify(x => x.Dispose(), Times.Once);
            second.WriteTransaction.Verify(x => x.Dispose(), Times.Once);
            first.Session.Verify(x => x.Dispose(), Times.Once);
            second.Session.Verify(x => x.Dispose(), Times.Once);
        }

        private sealed class SessionState
        {
            public Mock<ISession> Session { get; } = new Mock<ISession>();
            public Mock<ITransaction> WriteTransaction { get; } = Transaction();
            public Mock<ITransaction> ReadTransaction { get; } = Transaction();

            public SessionState()
            {
                Session.SetupSequence(x => x.BeginTransaction(IsolationLevel.ReadCommitted))
                    .Returns(WriteTransaction.Object).Returns(ReadTransaction.Object);
                Session.Setup(x => x.CreateSQLQuery("SET TRANSACTION READ ONLY")).Returns(Mock.Of<ISQLQuery>());
            }

            private static Mock<ITransaction> Transaction()
            {
                var transaction = new Mock<ITransaction>();
                var active = true;
                transaction.SetupGet(x => x.IsActive).Returns(() => active);
                transaction.Setup(x => x.Commit()).Callback(() => active = false);
                transaction.Setup(x => x.Rollback()).Callback(() => active = false);
                return transaction;
            }
        }
    }
}
