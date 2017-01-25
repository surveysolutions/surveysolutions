using System;
using System.IO;
using CommandLine;
using CommandLine.Text;
using System.Linq;
using System.Reflection;
using Humanizer;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Tool.hbm2ddl;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace A2DB
{
    class Program
    {
        protected class Options
        {
            [Option('p', "plainstore", Required = true, HelpText = "PostgreSql plain storage connection string.")]
            public string PGPlainConnection { get; set; }

            [Option('f', "folder", Required = true, HelpText = "Folder for assemblies to search in.")]
            public string Folder { get; set; }

            [ParserState]
            public IParserState LastParserState { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("This tool will scan and save Assemblies *.dll from folder to DB.");
            
            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine("Start.");
                SaveAssemblies(options);
                Console.WriteLine("Done.");
            }
            else
            {
                Console.WriteLine(
                    "Example: A2DB -p \"Server = 127.0.0.1; Port = 5432; User Id = postgres; Password = Qwerty1234; Database = SuperHQ-Plain\" -f \"c:temp\assemblies\"");
                
            }
        }

        private static void SaveAssemblies(Options options)
        {
            var plainPostgresTransactionManager = new PlainPostgresTransactionManager(BuildSessionFactory(options.PGPlainConnection));

            var assemblyStorage = new PostgresPlainStorageRepository<AssemblyInfo>(plainPostgresTransactionManager);

            var service = new AssemblyService(assemblyStorage);

            var assemblyFiles = Directory.GetFiles(options.Folder, "*.dll");

            foreach (var assemblyFile in assemblyFiles)
            {
                var fileCreated = File.GetCreationTime(assemblyFile);
                var fileName = Path.GetFileName(assemblyFile);
                var fileContent = File.ReadAllBytes(assemblyFile);
                service.SaveAssemblyInfo(fileName, fileCreated, fileContent);
            }
        }
       
        private static ISessionFactory BuildSessionFactory(string plainStorageConnection)
        {
            var cfg = new Configuration();
            cfg.DataBaseIntegration(db =>
            {
                db.ConnectionString = plainStorageConnection;
                db.Dialect<PostgreSQL91Dialect>();
                db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
            });

            cfg.AddDeserializedMapping(GetMappings(), "Plain");
            var update = new SchemaUpdate(cfg);
            update.Execute(true, true);

            return cfg.BuildSessionFactory();
        }

        private static HbmMapping GetMappings()
        {
            var mapper = new ModelMapper();
            var mappingTypes = new[] { typeof(AssemblyInfo).Assembly }
                .SelectMany(x => x.GetExportedTypes())
                .Where(x => x.GetCustomAttribute<PlainStorageAttribute>() != null &&
                            x.IsSubclassOfRawGeneric(typeof(ClassMapping<>)));

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
