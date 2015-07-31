using System;
using System.Configuration;
using System.Data.SqlClient;
using Machine.Specifications;
using Moq;
using NHibernate;
using Npgsql;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Storage.Postgre;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;
using WB.Core.SharedKernels.SurveySolutions;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.PostgreSQLTests
{
    public class TestPersistedClass : IReadSideRepositoryEntity
    {
        public DateTime Date { get; set; }
    }

    [Subject(typeof(PostgresReadSideKeyValueStorage<TestPersistedClass>))]
    public class when_storing_entity_into_key_value_storage
    {
        Establish context = () =>
        {
            configConnectionString = ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString;
            databaseName = "testdb_" + Guid.NewGuid().FormatGuid();
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(configConnectionString)
            {
                Database = databaseName
            };
            
            using (var connection = new NpgsqlConnection(configConnectionString))
            {
                connection.Open();
                var command = string.Format(@"CREATE DATABASE {0} ENCODING = 'UTF8'", databaseName);
                using (var sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = command;
                    sqlCommand.ExecuteNonQuery();
                }
                connection.Close();
            }
           
            pgSqlConnection = new NpgsqlConnection(connectionStringBuilder);
            pgSqlConnection.Open();

            usedConnectionString = connectionStringBuilder.ConnectionString;
            var sessionProvider = Mock.Of<ISessionProvider>(x => x.GetSession() == Mock.Of<ISession>(y => y.Transaction == Mock.Of<ITransaction>() && 
                y.Connection.CreateCommand() == pgSqlConnection.CreateCommand()));

            storage = new PostgresReadSideKeyValueStorage<TestPersistedClass>(sessionProvider, new PostgreConnectionSettings { ConnectionString = usedConnectionString });
            storedDate = new DateTime(2010, 1, 1);
            usedId = "id";
        };

        Because of = () => { storage.Store(new TestPersistedClass{Date = storedDate}, usedId); };

        It should_read_item_that_was_stored = () => storage.GetById(usedId).Date.ShouldEqual(storedDate);

        Cleanup things = () =>
        {
            pgSqlConnection.Close();

            using (var connection = new NpgsqlConnection(configConnectionString))
            {
                connection.Open();
                var command = @"DROP DATABASE " + databaseName;
                using (var sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = command;
                    sqlCommand.ExecuteNonQuery();
                }
            }
        };

        static PostgresReadSideKeyValueStorage<TestPersistedClass> storage;
        static string usedConnectionString;
        private static DateTime storedDate;
        private static string usedId;
        private static string databaseName;
        private static string configConnectionString;
        private static NpgsqlConnection pgSqlConnection;
    }
}

