using System;
using System.Collections.Generic;
using Dapper;
using NHibernate.Mapping.ByCode.Conformist;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Tests.Integration.PostgreSQLTests
{
    public class when_storing_TimeSpan : with_postgres_db
    {
        private class TestClass
        {
            public virtual int Id { get; set; }
            public virtual TimeSpan Duration { get; set; }
            public virtual TimeSpan? DurationNull { get; set; }
        }

        private class TestPersistedClassMap : ClassMapping<TestClass>
        {
            public TestPersistedClassMap()
            {
                Id(x => x.Id);
                Table("testclass_timespan");
                Property(x => x.Duration, m => m.Type<TimeSpanType>());
                Property(x => x.DurationNull, m =>
                {
                    m.Column("duration_null");
                    m.Type<TimeSpanType>();
                });
            }
        }

        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            var sessionFactory = IntegrationCreate.SessionFactory(ConnectionStringBuilder.ConnectionString,
                new List<Type>()
                {
                    typeof(TestPersistedClassMap)
                },
                true, null);

            CreateUnitOfWork = () => IntegrationCreate.UnitOfWork(sessionFactory);

            using (var unitOfWork = CreateUnitOfWork())
            {
                unitOfWork.Session.Connection.Execute("DROP TABLE if exists testclass_timespan;");
                unitOfWork.Session.Connection.Execute($@"CREATE TABLE testclass_timespan (
	                id serial NOT NULL,
	                duration interval NOT NULL,
	                duration_null interval NULL,
	                CONSTRAINT testclass_timespan_pk PRIMARY KEY (id));");

                unitOfWork.AcceptChanges();
            }
        }

        [Test]
        public void should_store_and_read_timespan_values()
        {
            var test = new TestClass
            {
                Duration = TimeSpan.FromMilliseconds(4234234234)
            };

            using (var u = CreateUnitOfWork())
            {
                u.Session.SaveOrUpdate(test);
                u.AcceptChanges();
            }

            using (var u = CreateUnitOfWork())
            {
                var item = u.Session.Get<TestClass>(test.Id);
                Assert.That(item.Duration, Is.EqualTo(test.Duration));
                ClassicAssert.Null(item.DurationNull);
            }
        }

        [Test]
        public void should_store_and_read_nullable_timespan_values()
        {
            var test = new TestClass
            {
                Duration = TimeSpan.FromMilliseconds(4234234234),
                DurationNull = TimeSpan.FromMilliseconds(23423523345)
            };

            using (var u = CreateUnitOfWork())
            {
                u.Session.SaveOrUpdate(test);
                u.AcceptChanges();
            }

            using (var u = CreateUnitOfWork())
            {
                var item = u.Session.Get<TestClass>(test.Id);
                Assert.That(item.DurationNull, Is.EqualTo(test.DurationNull));
            }
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            using (var unitOfWork = CreateUnitOfWork())
            {
                unitOfWork.Session.Connection.Execute("DROP TABLE if exists testclass_timespan;");
                unitOfWork.AcceptChanges();
            }
        }
        
        protected Func<IUnitOfWork> CreateUnitOfWork;
    }
}
