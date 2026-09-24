using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Npgsql;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;

namespace WB.Tests.Integration.PostgreSQLTests
{
    [TestFixture]
    public class UnitOfWorkTests : with_postgres_db
    {
        private class TestClass
        {
            public virtual Guid Id { get; set; }
        }

        private class TestPersistedClassMap : ClassMapping<TestClass>
        {
            public TestPersistedClassMap()
            {
                Id(x => x.Id, id => id.Generator(Generators.Assigned));
                Table("testclass_timespan_uow");
            }
        }

        protected Func<IUnitOfWork> CreateUnitOfWork;
        private ISessionFactory sessionFactory;

        [OneTimeSetUp]
        public void context()
        {
            sessionFactory = IntegrationCreate.SessionFactory(ConnectionStringBuilder.ConnectionString,
                new List<Type>
                {
                    typeof(TestPersistedClassMap),
                    typeof(SynchronizationLogItemMap)
                },
                true, null);

            CreateUnitOfWork = () => IntegrationCreate.UnitOfWork(sessionFactory);

            using (var session = sessionFactory.OpenStatelessSession())
            {
                using (var dbConnection = session.Connection)
                {
                    dbConnection.Execute("DROP TABLE if exists testclass_timespan_uow;");
                    dbConnection.Execute(@"CREATE TABLE testclass_timespan_uow (
	                id uuid NOT NULL,
	                CONSTRAINT testclass_timespan_pk PRIMARY KEY (id));");
                    dbConnection.Execute(@"CREATE TABLE testclass_uow_deferred (
                        id uuid PRIMARY KEY,
                        parent_id uuid REFERENCES testclass_timespan_uow(id) DEFERRABLE INITIALLY DEFERRED);");
                }
            }
        }

        [Test]
        public void should_not_commit_transaction_without_accept_changes_call()
        {
            var test = new TestClass
            {
                Id = Guid.NewGuid()
            };

            using (var u = CreateUnitOfWork())
            {
                u.Session.SaveOrUpdate(test);
            }

            using (var u = CreateUnitOfWork())
            {
                var item = u.Session.Get<TestClass>(test.Id);
                Assert.That(item, Is.Null);
            }
        }

        [Test]
        public void should_not_commit_transaction_after_discard_changes_call()
        {
            var test = new TestClass
            {
                Id = Guid.NewGuid()
            };

            using (var u = CreateUnitOfWork())
            {
                u.Session.SaveOrUpdate(test);
                u.DiscardChanges();
                u.AcceptChanges();
            }

            using (var u = CreateUnitOfWork())
            {
                var item = u.Session.Get<TestClass>(test.Id);
                Assert.That(item, Is.Null);
            }
        }

        [Test]
        public void should_commit_transaction_after_accept_changes_call()
        {
            var test = new TestClass
            {
                Id = Guid.NewGuid()
            };

            using (var u = CreateUnitOfWork())
            {
                u.Session.SaveOrUpdate(test);
                u.AcceptChanges();
            }

            using (var u = CreateUnitOfWork())
            {
                var item = u.Session.Get<TestClass>(test.Id);
                Assert.That(item, Is.Not.Null);
            }
        }

        [Test]
        public void should_commit_before_disposal_and_allow_rendering_reads()
        {
            var test = new TestClass { Id = Guid.NewGuid() };
            using var u = CreateUnitOfWork();
            var session = u.Session;
            session.SaveOrUpdate(test);
            u.AcceptChanges();

            u.Complete();

            using (var reader = CreateUnitOfWork())
                Assert.That(reader.Session.Get<TestClass>(test.Id), Is.Not.Null);

            Assert.That(u.Session, Is.SameAs(session));
            session.Clear();
            Assert.That(session.Get<TestClass>(test.Id), Is.Not.Null);
            Assert.That(session.CreateSQLQuery("SHOW transaction_read_only").UniqueResult<string>(), Is.EqualTo("on"));
            Assert.DoesNotThrow(() => u.Complete());
        }

        [Test]
        public void should_roll_back_discarded_changes_during_explicit_completion()
        {
            var test = new TestClass { Id = Guid.NewGuid() };
            using var u = CreateUnitOfWork();
            u.Session.SaveOrUpdate(test);
            u.Session.Flush();
            u.DiscardChanges();
            u.AcceptChanges();

            u.Complete();

            using var reader = CreateUnitOfWork();
            Assert.That(reader.Session.Get<TestClass>(test.Id), Is.Null);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void should_reject_writes_after_completion_even_for_new_sessions(bool openBeforeCompletion)
        {
            var id = Guid.NewGuid();
            using var u = CreateUnitOfWork();
            if (openBeforeCompletion) _ = u.Session;
            u.AcceptChanges();
            u.Complete();

            var exception = Assert.Catch(() => u.Session
                .CreateSQLQuery("INSERT INTO testclass_timespan_uow (id) VALUES (:id)")
                .SetParameter("id", id).ExecuteUpdate());

            Assert.That(exception.GetBaseException(), Is.InstanceOf<PostgresException>());
            Assert.That(((PostgresException)exception.GetBaseException()).SqlState, Is.EqualTo("25006"));
            Assert.DoesNotThrow(() => u.Dispose());
            using var reader = CreateUnitOfWork();
            Assert.That(reader.Session.Get<TestClass>(id), Is.Null);
        }

        [Test]
        public void should_surface_deferred_constraint_failure_at_completion_not_disposal()
        {
            var id = Guid.NewGuid();
            using var u = CreateUnitOfWork();
            // This insert succeeds; the foreign key is checked only when COMMIT executes.
            u.Session.CreateSQLQuery("INSERT INTO testclass_uow_deferred (id, parent_id) VALUES (:id, :parent)")
                .SetParameter("id", id).SetParameter("parent", Guid.NewGuid()).ExecuteUpdate();
            u.AcceptChanges();

            var exception = Assert.Catch(() => u.Complete());

            Assert.That(exception.GetBaseException(), Is.InstanceOf<PostgresException>());
            Assert.That(((PostgresException)exception.GetBaseException()).SqlState, Is.EqualTo("23503"));
            Assert.Throws<InvalidOperationException>(() => u.Complete());
            Assert.DoesNotThrow(() => u.Dispose());
            using var reader = CreateUnitOfWork();
            Assert.That(reader.Session.CreateSQLQuery("SELECT count(*) FROM testclass_uow_deferred WHERE id = :id")
                .SetParameter("id", id).UniqueResult<long>(), Is.Zero);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task should_persist_sync_failure_log_while_rolling_back_request_changes(bool abortTransaction)
        {
            SynchronizationLogItem savedLog = null;
            IUnitOfWork loggingUnitOfWork = null;
            var workspaceSetter = new Mock<IWorkspaceContextSetter>();
            var builder = new ContainerBuilder();
            builder.RegisterInstance(sessionFactory).As<ISessionFactory>().ExternallyOwned();
            builder.RegisterInstance(NullLogger<UnitOfWork>.Instance).As<Microsoft.Extensions.Logging.ILogger<UnitOfWork>>();
            builder.RegisterInstance(Mock.Of<IWorkspaceContextAccessor>(x => x.CurrentWorkspace() == WorkspaceContext.Default));
            builder.RegisterInstance(workspaceSetter.Object);
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();
            builder.RegisterType<UnitOfWorkInScopeExecutor<IPlainStorageAccessor<SynchronizationLogItem>>>()
                .As<IInScopeExecutor<IPlainStorageAccessor<SynchronizationLogItem>>>();
            builder.Register(c =>
            {
                var unitOfWork = c.Resolve<IUnitOfWork>();
                var storage = new Mock<IPlainStorageAccessor<SynchronizationLogItem>>();
                storage.Setup(x => x.Store(It.IsAny<SynchronizationLogItem>(), It.IsAny<object>()))
                    .Callback<SynchronizationLogItem, object>((item, _) =>
                    {
                        loggingUnitOfWork = unitOfWork;
                        savedLog = item;
                        unitOfWork.Session.SaveOrUpdate(item);
                    });
                return storage.Object;
            }).As<IPlainStorageAccessor<SynchronizationLogItem>>().InstancePerLifetimeScope();
            using var container = builder.Build();
            var changed = new TestClass { Id = Guid.NewGuid() };
            Exception actionException = new InvalidOperationException("Synchronization failed");

            using (var requestScope = container.BeginLifetimeScope())
            {
                var requestUnitOfWork = requestScope.Resolve<IUnitOfWork>();
                var http = new DefaultHttpContext { RequestServices = new AutofacServiceProvider(requestScope) };
                var context = new ActionExecutingContext(new ActionContext(http, new RouteData(), new ActionDescriptor()),
                    new List<IFilterMetadata>(), new Dictionary<string, object> { ["id"] = "device-123" }, new object());
                var executed = new ActionExecutedContext(context, context.Filters, context.Controller);

                await new UnitOfWorkActionFilter().OnActionExecutionAsync(context, async () =>
                {
                    await new WriteToSyncLogAttribute(SynchronizationLogType.LinkToDevice)
                        .OnActionExecutionAsync(context, () =>
                        {
                            requestUnitOfWork.Session.SaveOrUpdate(changed);
                            requestUnitOfWork.Session.Flush();
                            requestUnitOfWork.AcceptChanges(); // Failure must override an earlier acceptance.
                            if (abortTransaction)
                                actionException = Assert.Catch(() => requestUnitOfWork.Session.CreateSQLQuery("SELECT 1 / 0").UniqueResult());
                            executed.Exception = actionException;
                            return Task.FromResult(executed);
                        });
                    return executed;
                });

                Assert.That(executed.Exception, Is.SameAs(actionException));
                Assert.That(executed.ExceptionHandled, Is.False);
                Assert.That(requestUnitOfWork.Session.IsOpen, Is.True);
                Assert.That(savedLog, Is.Not.Null);
                Assert.That(loggingUnitOfWork, Is.Not.SameAs(requestUnitOfWork));
                workspaceSetter.Verify(x => x.Set(WorkspaceContext.Default), Times.Once);
            }

            using var reader = CreateUnitOfWork();
            Assert.That(reader.Session.Get<TestClass>(changed.Id), Is.Null);
            var persistedLog = reader.Session.Get<SynchronizationLogItem>(savedLog.Id);
            Assert.That(persistedLog, Is.Not.Null);
            Assert.That(persistedLog.ActionExceptionMessage, Is.EqualTo(actionException.Message));
            Assert.That(persistedLog.ActionExceptionType, Is.EqualTo(actionException.GetType().Name));
        }
    }
}
