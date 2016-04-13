using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CommandLine;
using CommandLine.Text;
using Humanizer;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Main.Core.Events.User;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json.Linq;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Tool.hbm2ddl;
using Npgsql;
using NpgsqlTypes;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.Synchronization.Events.Sync;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Storage.EventStore;
using WB.Infrastructure.Native.Storage.EventStore.Implementation;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace EventStoreToPlainStorageMigrator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("This tool will migrate questionnaires or usersor both, depends on parameter 'c'(entitytype)");
            Console.WriteLine("If entity type is equal to 'q' the tool will migrate questionnaires, if 'u' then users, if 'both' the questionnaires and users");
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine("Start.");
                Transfer(options);
                Console.WriteLine("Done.");
            }
            else
            {
                Console.WriteLine(
                    "Example when EventStore stores events and questionnaires need to be migrated: EventStoreToPlainStorageMigrator -c \"q\" -p \"Server = 127.0.0.1; Port = 5432; User Id = postgres; Password = Qwerty1234; Database = SuperHQ-Plain\" -i \"127.0.0.1\" -t 1113 -h 2113 -l admin -s changeit");

                Console.WriteLine(
                    "Example when Postgres stores events  and users need to be migrated: EventStoreToPlainStorageMigrator -c \"u\" -p \"Server = 127.0.0.1; Port = 5432; User Id = postgres; Password = Qwerty1234; Database = SuperHQ-Plain\" -e \"Server = 127.0.0.1; Port = 5432; User Id = postgres; Password = Qwerty1234; Database = SuperHQ-Events\"");

            }
        }

        protected class Options
        {
            [Option('i', "eventstoreserverip", Required = false, HelpText = "Event store server ip.")]
            public string ServerIP { get; set; }

            [Option('t', "eventstoreservertcp", Required = false, HelpText = "Event store server tct.")]
            public string ServerTcpPort { get; set; }

            [Option('h', "eventstoreserverhttpport", Required = false, HelpText = "Event store http port.")]
            public string ServerHttpPort { get; set; }

            [Option('l', "eventstorelogin", Required = false, HelpText = "Event store login.")]
            public string Login { get; set; }

            [Option('s', "eventstorepassword", Required = false, HelpText = "Event store password.")]
            public string Password { get; set; }

            [Option('p', "plainstore", Required = true, HelpText = "PostgreSql plain storage connection string.")]
            public string PGPlainConnection { get; set; }

            [Option('e', "postgreseventstore", Required = false, HelpText = "Event store server ip.")]
            public string PostgresEventStore { get; set; }

            [Option('c', "entitytype", Required = true, HelpText = "What entity to migrate.")]
            public string EntityTypeToMigrate { get; set; }

            [ParserState]
            public IParserState LastParserState { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this,
                    (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }

        private static void Transfer(Options options)
        {
            var logger = Mock.Of<WB.Core.GenericSubdomains.Portable.Services.ILogger>();

            var eventTypeResolver = CreateEventTypeResolver();

            IStreamableEventStore eventStore;
            if (string.IsNullOrEmpty(options.PostgresEventStore))
            {
                var eventStoreSettings = new EventStoreSettings()
                {
                    ServerIP = options.ServerIP,
                    ServerTcpPort = int.Parse(options.ServerTcpPort),
                    ServerHttpPort = int.Parse(options.ServerHttpPort),
                    Login = options.Login,
                    Password = options.Password
                };

                eventStore = new WriteSideEventStore(new EventStoreConnectionProvider(eventStoreSettings), logger,
                    eventStoreSettings, eventTypeResolver);
            }
            else
            {
                eventStore =
                    new PostgresEventStore(
                        new PostgreConnectionSettings() {ConnectionString = options.PostgresEventStore},
                        eventTypeResolver);
            }

            var postgresPlainStorageSettings = new PostgresPlainStorageSettings()
            {
                ConnectionString = options.PGPlainConnection
            };
            var plainPostgresTransactionManager =
                new PlainPostgresTransactionManager(BuildSessionFactory(options.PGPlainConnection));

            ObsoleteEventHandleDescriptor[] eventHandlers;

            if (options.EntityTypeToMigrate == "q")
            {
                eventHandlers=new QuestionnaireMigrationEventHandlers(plainPostgresTransactionManager,logger, postgresPlainStorageSettings).eventHandlers;
            }
            else if(options.EntityTypeToMigrate == "u")
            {
                eventHandlers=new UserMigrationEventHandlers(plainPostgresTransactionManager).eventHandlers;
            }
            else if (options.EntityTypeToMigrate == "both")
            {
                var userEventHandlers = new UserMigrationEventHandlers(plainPostgresTransactionManager).eventHandlers;
                var questionnaireEventHandlers = new QuestionnaireMigrationEventHandlers(plainPostgresTransactionManager, logger, postgresPlainStorageSettings).eventHandlers;
                eventHandlers = userEventHandlers.Union(questionnaireEventHandlers).ToArray();
            }
            else
            {
                throw new ArgumentException(
                    $"entity type {options.EntityTypeToMigrate} is unknown, 'q'-questionnaire and 'u'- users are only available types");
            }

            MigrateEvents(eventStore, plainPostgresTransactionManager, eventHandlers);
        }

        private static void MigrateEvents(IStreamableEventStore eventStore,
            PlainPostgresTransactionManager plainPostgresTransactionManager,
            ObsoleteEventHandleDescriptor[] eventHandlers)
        {
            MoveEventsToPlainStorage(eventStore, plainPostgresTransactionManager, eventHandlers);
        }

        private static HbmMapping GetMappings()
        {
            var mapper = new ModelMapper();
            var mappingTypes = new[] {typeof (QuestionnaireBrowseItem).Assembly}
                .SelectMany(x => x.GetExportedTypes())
                .Where(x => x.GetCustomAttribute<PlainStorageAttribute>() != null &&
                            x.IsSubclassOfRawGeneric(typeof (ClassMapping<>)));

            mapper.AddMappings(mappingTypes);
            mapper.BeforeMapProperty += (inspector, member, customizer) =>
            {
                var propertyInfo = (PropertyInfo) member.LocalMember;
                if (propertyInfo.PropertyType == typeof (string))
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

        private static void MoveEventsToPlainStorage(IStreamableEventStore eventStore, PlainPostgresTransactionManager plainPostgresTransactionManager, ObsoleteEventHandleDescriptor[] eventHandlers)
        {
            var events = eventStore.GetAllEvents();
            var eventsCount = eventStore.CountOfAllEvents();

            var countOfScannedEvents = 0;
            foreach (var committedEvent in events)
            {
                foreach (var eventHandler in eventHandlers)
                {
                    eventHandler.Handle(committedEvent, plainPostgresTransactionManager);
                }
                countOfScannedEvents++;

                if (countOfScannedEvents % 5000 == 0)
                {
                    Console.WriteLine(@"Scanned {0} out of {1}", countOfScannedEvents, eventsCount);
                }
            }
        }

        private static EventTypeResolver CreateEventTypeResolver()
        {
            var eventTypeResolver = new EventTypeResolver();
            var eventNamespaces = new[]
            {"WB.Core.SharedKernels.DataCollection.Events", "Main.Core.Events", "WB.Core.Synchronization.Events"};
            var assemlies = new[] {typeof (TabletRegistered).Assembly, typeof (NewUserCreated).Assembly};
            var events =
                assemlies.SelectMany(a => a.GetTypes())
                    .Where(
                        t => t.Namespace != null &&
                             eventNamespaces.Any(
                                 eventNamespace => t.Namespace.StartsWith(eventNamespace, StringComparison.Ordinal)))
                    .ToArray();

            foreach (var @event in events)
            {
                eventTypeResolver.RegisterEventDataType(@event);
            }
            return eventTypeResolver;
        }
    }
}
