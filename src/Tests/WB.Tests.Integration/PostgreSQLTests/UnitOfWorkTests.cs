using System;
using System.Collections.Generic;
using Dapper;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
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
    }
}
