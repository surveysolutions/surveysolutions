using System;
using System.Collections.Generic;
using Dapper;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Npgsql;
using NUnit.Framework;
using WB.Infrastructure.Native.Storage.Postgre;

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

        [OneTimeSetUp]
        public void context()
        {
            var sessionFactory = IntegrationCreate.SessionFactory(ConnectionStringBuilder.ConnectionString,
                new List<Type>
                {
                    typeof(TestPersistedClassMap)
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
    }
}
