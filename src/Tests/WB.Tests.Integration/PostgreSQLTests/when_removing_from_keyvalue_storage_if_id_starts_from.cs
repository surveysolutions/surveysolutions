using FluentAssertions;
using Moq;
using NHibernate;
using Npgsql;
using NUnit.Framework;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Tests.Integration.PostgreSQLTests
{
    [TestOf(typeof(PostgresReadSideKeyValueStorage<TestRemoveKeyValueStartsFromClass>))]
    public class when_removing_from_keyvalue_storage_if_id_starts_from : with_postgres_db
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            pgSqlConnection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
            pgSqlConnection.Open();

            var sessionProvider =
                Mock.Of<ISessionProvider>(
                    x => x.GetSession() == Mock.Of<ISession>(y => y.Transaction == Mock.Of<ITransaction>() && y.Connection == pgSqlConnection));
            storage = IntegrationCreate.PostgresReadSideKeyValueStorage<TestRemoveKeyValueStartsFromClass>(
                sessionProvider: sessionProvider, postgreConnectionSettings: new PostgreConnectionSettings { ConnectionString = connectionStringBuilder.ConnectionString });

            storage.Store(new TestRemoveKeyValueStartsFromClass { Value = "test1" }, $"{nastya}1");
            storage.Store(new TestRemoveKeyValueStartsFromClass { Value = "test2" }, $"{nastya}2");
            storage.Store(new TestRemoveKeyValueStartsFromClass { Value = "test3" }, "vitaliy");
            BecauseOf();
        }

        public void BecauseOf() { storage.RemoveIfStartsWith(nastya); }

        [NUnit.Framework.Test] public void should_nastya1_value_be_null () => storage.GetById($"{nastya}1").Should().NotBeNull();

        [NUnit.Framework.Test] public void should_nastya2_value_be_null () => storage.GetById($"{nastya}2").Should().NotBeNull();

        [NUnit.Framework.Test] public void should_vitaliy_value_be_not_null () => storage.GetById("vitaliy").Should().NotBeNull();

        
        [OneTimeTearDown]
        public void TearDown()
        {
            pgSqlConnection.Close();
        }

        static PostgresReadSideKeyValueStorage<TestRemoveKeyValueStartsFromClass> storage;
        static string nastya = "nastya";
        static NpgsqlConnection pgSqlConnection;
    }

    public class TestRemoveKeyValueStartsFromClass : IReadSideRepositoryEntity
    {
        public string Value { get; set; }
    }

}
