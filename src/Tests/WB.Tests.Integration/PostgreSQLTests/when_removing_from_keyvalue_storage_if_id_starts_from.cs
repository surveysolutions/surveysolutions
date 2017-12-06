using System;
using Machine.Specifications;
using Moq;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using Npgsql;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.PostgreSQLTests
{
    [Subject(typeof(PostgresReadSideKeyValueStorage<TestRemoveKeyValueStartsFromClass>))]
    public class when_removing_from_keyvalue_storage_if_id_starts_from : with_postgres_db
    {
        Establish context = () =>
        {
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
        };

        Because of = () => { storage.RemoveIfStartsWith(nastya); };

        It should_nastya1_value_be_null = () => storage.GetById($"{nastya}1").ShouldNotBeNull();

        It should_nastya2_value_be_null = () => storage.GetById($"{nastya}2").ShouldNotBeNull();

        It should_vitaliy_value_be_not_null = () => storage.GetById("vitaliy").ShouldNotBeNull();

        Cleanup things = () => { pgSqlConnection.Close(); };

        static PostgresReadSideKeyValueStorage<TestRemoveKeyValueStartsFromClass> storage;
        static string nastya = "nastya";
        static NpgsqlConnection pgSqlConnection;
    }

    public class TestRemoveKeyValueStartsFromClass : IReadSideRepositoryEntity
    {
        public string Value { get; set; }
    }

}