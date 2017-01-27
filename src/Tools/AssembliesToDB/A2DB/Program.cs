using System;
using System.IO;
using CommandLine;
using CommandLine.Text;
using System.Linq;
using System.Reflection;
using Humanizer;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NinjectAdapter;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace A2DB
{
    class Program
    {
        protected class Options
        {
            [Option('p', "plainstore", Required = true, HelpText = "PostgreSql storage connection string.")]
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
            var schemaName = "plainstore";
            var plainPostgresTransactionManager = new PlainPostgresTransactionManager(BuildSessionFactory(schemaName, options.PGPlainConnection));

            var assemblyStorage = new PostgresPlainStorageRepository<AssemblyInfo>(plainPostgresTransactionManager);


            var serviceLocator = new Mock<IServiceLocator>();

            var logger = new Mock<ILogger>();
            
            serviceLocator.Setup(locator => locator.GetInstance<ILogger>()).Returns(Mock.Of<ILogger>());

            var loggerProvider = new Mock<ILoggerProvider>();
            loggerProvider.Setup(provider => provider.GetForType(typeof(DbMigrationsRunner))).Returns(logger.Object);

            serviceLocator.Setup(locator => locator.GetInstance<ILoggerProvider>())
                .Returns(Mock.Of<ILoggerProvider>());
            

            ServiceLocator.SetLocatorProvider(() => serviceLocator.Object);
            
            WB.Infrastructure.Native.Storage.Postgre.DbMigrations.DbMigrationsRunner.MigrateToLatest(
                options.PGPlainConnection,
                schemaName,
                new DbUpgradeSettings(
                    typeof(WB.UI.Headquarters.Migrations.PlainStore.M001_Init).Assembly, 
                    typeof(WB.UI.Headquarters.Migrations.PlainStore.M001_Init).Namespace));

            var service = new AssemblyService(assemblyStorage);

            var assemblyFiles = Directory.GetFiles(options.Folder, "*.dll");

            foreach (var assemblyFile in assemblyFiles)
            {
                var fileCreated = File.GetCreationTime(assemblyFile);
                var fileName = Path.GetFileName(assemblyFile);
                var fileContent = File.ReadAllBytes(assemblyFile);

                Console.WriteLine("Saving " + fileName);

                plainPostgresTransactionManager.ExecuteInPlainTransaction(() =>
                {
                    service.SaveAssemblyInfo(fileName, fileCreated, fileContent);
                });

                File.Move(assemblyFile, assemblyFile + ".moved");
            }
        }


        private static ISessionFactory BuildSessionFactory(string schemaName, string plainStorageConnection)
        {
            var cfg = new Configuration();
            cfg.DataBaseIntegration(db =>
            {
                db.ConnectionString = plainStorageConnection;
                db.Dialect<PostgreSQL91Dialect>();
                db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
            });

            cfg.AddDeserializedMapping(GetMappings(schemaName), "Plain");
            cfg.SetProperty(NHibernate.Cfg.Environment.DefaultSchema, schemaName);
            return cfg.BuildSessionFactory();
        }

        private static HbmMapping GetMappings(string schemaName)
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
                customizer.Schema(schemaName);
            };
            mapper.BeforeMapSet += (inspector, member, customizer) => customizer.Schema(schemaName);
            mapper.BeforeMapBag += (inspector, member, customizer) => customizer.Schema(schemaName);
            mapper.BeforeMapList += (inspector, member, customizer) => customizer.Schema(schemaName);

            var map = mapper.CompileMappingForAllExplicitlyAddedEntities();

            return map;

        }
    }
}
