using System.Reflection;
using Machine.Specifications;
using Moq;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Persister.Entity;
using NHibernate.Tool.hbm2ddl;
using Npgsql;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.PostgreSQLTests
{
    [Subject(typeof(PostgresReadSideKeyValueStorage<TestRemoveStartsFrom>))]
    public class when_removing_from_repository_storage_if_id_starts_from : with_postgres_db
    {
        Establish context = () =>
        {
            pgSqlConnection = new NpgsqlConnection(connectionStringBuilder);
            pgSqlConnection.Open();

            var cfg = new Configuration();
            cfg.DataBaseIntegration(db =>
            {
                db.ConnectionString = TestConnectionString;
                db.Dialect<PostgreSQL91Dialect>();
                db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
            });
            cfg.AddDeserializedMapping(CreateMapping(), "Main");
            var update = new SchemaUpdate(cfg);
            update.Execute(true, true);
            var sessionFactory = cfg.BuildSessionFactory();
            var session = sessionFactory.OpenSession();

            var sessionProvider =
                Mock.Of<ISessionProvider>(
                    x =>
                        x.GetSession() == session);

            storage = IntegrationCreate.PostgresReadSideRepository<TestRemoveStartsFrom>(sessionProvider: sessionProvider, idColumnName: "EntityId");

            storage.Store(new TestRemoveStartsFrom { Value = "test1", EntityId = $"{nastya}1" }, $"{nastya}1");
            storage.Store(new TestRemoveStartsFrom { Value = "test2", EntityId = $"{nastya}2" }, $"{nastya}2");
            storage.Store(new TestRemoveStartsFrom { Value = "test3", EntityId = $"vitaliy" }, "vitaliy");
        };

        Because of = () => { storage.RemoveIfStartsWith(nastya); };

        It should_nastya1_value_be_null = () => storage.GetById($"{nastya}1").ShouldNotBeNull();

        It should_nastya2_value_be_null = () => storage.GetById($"{nastya}2").ShouldNotBeNull();

        It should_vitaliy_value_be_not_null = () => storage.GetById("vitaliy").ShouldNotBeNull();

        Cleanup things = () => { pgSqlConnection.Close(); };

        static PostgreReadSideStorage<TestRemoveStartsFrom> storage;
        static string nastya = "nastya";
        static NpgsqlConnection pgSqlConnection;

        private static HbmMapping CreateMapping()
        {
            var mapper = new ModelMapper();
            mapper.AddMapping(typeof(TestRemoveStartsFromClassMap));
            mapper.BeforeMapProperty += (inspector, member, customizer) =>
            {
                var propertyInfo = (PropertyInfo)member.LocalMember;
                if (propertyInfo.PropertyType == typeof(string))
                {
                    customizer.Type(NHibernateUtil.StringClob);
                }
            };

            mapper.BeforeMapClass += (inspector, type, customizer) =>
            {
                var tableName = "TestRemoveStartsFroms";
                customizer.Table(tableName);
            };

            return mapper.CompileMappingForAllExplicitlyAddedEntities();
        }
    }

    public class TestRemoveStartsFrom : IReadSideRepositoryEntity
    {
        public virtual string EntityId { get; set; }
        public virtual string Value { get; set; }
    }

    public class TestRemoveStartsFromClassMap : ClassMapping<TestRemoveStartsFrom>
    {
        public TestRemoveStartsFromClassMap()
        {
            Id(x => x.EntityId, idMap => idMap.Generator(Generators.Assigned));

            Property(x => x.Value);
        }
    }
}