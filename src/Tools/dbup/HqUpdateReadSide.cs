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
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace dbup
{
    [Description("Updates read side to a state of 5.10 release for later migrations")]
    internal class HqUpdateReadSide : IConsoleCommand
    {
        [Description("Read side connection string")]
        [Argument(Name = "cs")]
        public string ConnectionString { get; set; }

        public Task RunAsync(CommandLineProcessor processor, IConsoleHost host)
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

            SchemaExport export = new SchemaExport(cfg);
            export.SetOutputFile("init-read.sql");
            export.Create(false, false);

            return Task.FromResult(true);
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