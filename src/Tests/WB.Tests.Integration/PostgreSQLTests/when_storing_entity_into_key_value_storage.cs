using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Tests.Integration.PostgreSQLTests
{
    public class TestPersistedClass : IReadSideRepositoryEntity
    {
        public DateTime Date { get; set; }
    }

    [TestOf(typeof(PostgresReadSideKeyValueStorage<TestPersistedClass>))]
    public class when_storing_entity_into_key_value_storage : with_postgres_db
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            var sessionFactory = IntegrationCreate.SessionFactory(ConnectionStringBuilder.ConnectionString,
                new List<Type>()
                {
                    typeof(TestPersistedClass)
                },
                true, null);

            UnitOfWork = IntegrationCreate.UnitOfWork(sessionFactory);
            
            storage = IntegrationCreate.PostgresReadSideKeyValueStorage<TestPersistedClass>(
                sessionProvider: UnitOfWork, 
                postgreConnectionSettings: new UnitOfWorkConnectionSettings { ConnectionString = ConnectionStringBuilder.ConnectionString });
            storedDate = new DateTime(2010, 1, 1);
            usedId = "id";

            BecauseOf();
        }

        public void BecauseOf() => storage.Store(new TestPersistedClass { Date = storedDate }, usedId); 

        [NUnit.Framework.Test] public void should_read_item_that_was_stored() => storage.GetById(usedId).Date.Should().Be(storedDate);

        [OneTimeTearDown]
        public void TearDown()
        {
            UnitOfWork.Dispose();
        }

        static PostgresReadSideKeyValueStorage<TestPersistedClass> storage;
        static DateTime storedDate;
        static string usedId;
        protected IUnitOfWork UnitOfWork;
    }
}

