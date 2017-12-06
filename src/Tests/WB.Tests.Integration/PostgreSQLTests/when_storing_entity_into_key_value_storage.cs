using System;
using System.Data.SqlClient;
using Machine.Specifications;
using Moq;
using NHibernate;
using Npgsql;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.PostgreSQLTests
{
    public class TestPersistedClass : IReadSideRepositoryEntity
    {
        public DateTime Date { get; set; }
    }

    [Subject(typeof(PostgresReadSideKeyValueStorage<TestPersistedClass>))]
    public class when_storing_entity_into_key_value_storage : with_postgres_db
    {
        Establish context = () =>
        {
            pgSqlConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
            pgSqlConnection.Open();

            var sessionProvider = Mock.Of<ISessionProvider>(x => x.GetSession() == Mock.Of<ISession>(y => y.Transaction == Mock.Of<ITransaction>() && y.Connection == pgSqlConnection));
            storage = IntegrationCreate.PostgresReadSideKeyValueStorage<TestPersistedClass>(
                sessionProvider: sessionProvider, postgreConnectionSettings: new PostgreConnectionSettings { ConnectionString = connectionStringBuilder.ConnectionString });
            storedDate = new DateTime(2010, 1, 1);
            usedId = "id";
        };

        Because of = () => { storage.Store(new TestPersistedClass{Date = storedDate}, usedId); };

        It should_read_item_that_was_stored = () => storage.GetById(usedId).Date.ShouldEqual(storedDate);

        Cleanup things = () => { pgSqlConnection.Close(); };

        static PostgresReadSideKeyValueStorage<TestPersistedClass> storage;
        static DateTime storedDate;
        static string usedId;
        static NpgsqlConnection pgSqlConnection;
    }
}

