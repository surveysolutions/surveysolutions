using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Humanizer;
using NConsole;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Tool.hbm2ddl;
using Npgsql;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;
using Environment = NHibernate.Cfg.Environment;

namespace dbup
{
    [Description("Updates read side to a state of 5.10 release for later migrations")]
    internal class HqUpdateReadSide : IConsoleCommand
    {
        [Description("Read side connection string")]
        [Argument(Name = "cs")]
        public string ConnectionString { get; set; }

        public async Task RunAsync(CommandLineProcessor processor, IConsoleHost host)
        {
            var cfg = new Configuration();
            cfg.DataBaseIntegration(db =>
            {
                db.ConnectionString = this.ConnectionString;
                db.Dialect<PostgreSQL91Dialect>();
                db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
            });
            cfg.SetProperty(Environment.WrapResultSets, "true");
            cfg.AddDeserializedMapping(this.GetMappings(), "Main");
            var update = new SchemaUpdate(cfg);
            update.Execute(true, true);


            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"CREATE TABLE IF NOT EXISTS ""VersionInfo""
(
  ""Version"" bigint NOT NULL,
  ""AppliedOn"" timestamp without time zone,
  ""Description"" character varying(1024)
)";
                await command.ExecuteNonQueryAsync();
                var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = @"INSERT INTO ""VersionInfo"" VALUES(:version, :timeStamp, :desc);";
                insertCommand.Parameters.AddWithValue("version", 1);
                insertCommand.Parameters.AddWithValue("timeStamp", DateTime.UtcNow);
                insertCommand.Parameters.AddWithValue("desc", "marked as 0 state for db");
                await insertCommand.ExecuteNonQueryAsync();
            }
        }

        private HbmMapping GetMappings()
        {
            var mapper = new ModelMapper();
            var mappingAssemblies = new List<Assembly> { typeof(SurveyManagementSharedKernelModule).Assembly, typeof(HeadquartersBoundedContextModule).Assembly };
            var mappingTypes = mappingAssemblies.SelectMany(x => x.GetExportedTypes())
                                                .Where(x => x.GetCustomAttribute<PlainStorageAttribute>() == null && x.IsSubclassOfRawGeneric(typeof(ClassMapping<>)));
            mapper.AddMappings(mappingTypes);
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
                var tableName = type.Name.Pluralize();
                customizer.Table(tableName);
            };

            return mapper.CompileMappingForAllExplicitlyAddedEntities();
        }
    }
}