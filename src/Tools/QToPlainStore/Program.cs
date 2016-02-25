using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Reflection;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Humanizer;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Main.Core.Events.User;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Tool.hbm2ddl;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Commands;
using WB.Core.SharedKernels.SurveyManagement.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.Synchronization.Events.Sync;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Storage.EventStore;
using WB.Infrastructure.Native.Storage.EventStore.Implementation;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace QToPlainStore
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine("Start.");
                Transfer(options);
                Console.WriteLine("Done.");
            }
        }

        protected class Options
        {
            [Option('r', "readside", Required = false, HelpText = "PostgreSql read side connection string.")]
            public string PGReadSideConnection { get; set; }
            
            [Option('i',"eventstoreserverip", Required = false, HelpText = "Event store server ip.")]
            public string ServerIP { get; set; }
            [Option('t',"eventstoreservertcp", Required = false, HelpText = "Event store server tct.")]
            public string ServerTcpPort { get; set; }
            [Option('h',"eventstoreserverhttpport", Required = false, HelpText = "Event store http port.")]
            public string ServerHttpPort { get; set; }
            [Option('l',"eventstorelogin", Required = false, HelpText = "Event store login.")]
            public string Login { get; set; }
            [Option('s',"eventstorepassword", Required = false, HelpText = "Event store password.")]
            public string Password { get; set; }
            [Option('p', "plainstore", Required = true, HelpText = "PostgreSql plain storage connection string.")]
            public string PGPlainConnection { get; set; }

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
            if (!string.IsNullOrEmpty(options.PGReadSideConnection))
            {
                var items = GetQuestionnairesFromReadSide(options.PGReadSideConnection);
                Console.WriteLine("Found {0} questionnaires.", items.Count);

                foreach (var item in items)
                {
                    Console.WriteLine("Processing {0} questionnaire.", item.Key);
                    InsertItemIfNotExists(options.PGPlainConnection, item.Key, item.Value);
                }
            }
            else
            {
                MigrateEvents(options.ServerIP, int.Parse(options.ServerTcpPort), int.Parse(options.ServerHttpPort), options.Login,
                    options.Password, options.PGPlainConnection);
            }
        }

        private static void MigrateEvents(string serverip, int serverTcp, int serverHttp, string login, string password, string plainStorageConnection)
        {
            var eventStoreSettings = new EventStoreSettings()
            {
                ServerIP = serverip,
                ServerTcpPort = serverTcp,
                ServerHttpPort = serverHttp,
                Login = login,
                Password = password,
                UseBson = false
            };

            var eventTypeResolver = CreateEventTypeResolver();

            var logger = Mock.Of<WB.Core.GenericSubdomains.Portable.Services.ILogger>();
            var serializer = new NewtonJsonSerializer(new JsonSerializerSettingsFactory());
            var eventStore = new WriteSideEventStore(new EventStoreConnectionProvider(eventStoreSettings),
                logger, eventStoreSettings, eventTypeResolver, serializer);

            var postgresPlainStorageSettings = new PostgresPlainStorageSettings()
            {
                ConnectionString = plainStorageConnection
            };
            plainPostgresTransactionManager =new PlainPostgresTransactionManager(BuildSessionFactory(plainStorageConnection));
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentRepository =
                new PostgresPlainKeyValueStorage<QuestionnaireDocument>(null, postgresPlainStorageSettings, logger, serializer);
            plainQuestionnaireRepository = new PlainQuestionnaireRepositoryWithCache(questionnaireDocumentRepository);

            questionnaireExportStructureStorage=new PostgresPlainKeyValueStorage<QuestionnaireExportStructure>(null, postgresPlainStorageSettings, logger, serializer);
            questionnaireRosterStructureStorage=new PostgresPlainKeyValueStorage<QuestionnaireRosterStructure>(null, postgresPlainStorageSettings, logger, serializer);
            referenceInfoForLinkedQuestionsStorage=new PostgresPlainKeyValueStorage<ReferenceInfoForLinkedQuestions>(null, postgresPlainStorageSettings, logger, serializer);
            questionnaireQuestionsInfoStorage=new PostgresPlainKeyValueStorage<QuestionnaireQuestionsInfo>(null, postgresPlainStorageSettings, logger, serializer);

            questionnaireBrowseItemStorage=new PostgresPlainStorageRepository<QuestionnaireBrowseItem>(plainPostgresTransactionManager);

            MoveQuestionnaireEventsToPlainStorage(eventStore);

        }
        private static HbmMapping GetMappings()
        {
            var mapper = new ModelMapper();
            var mappingTypes = new[] {typeof(QuestionnaireBrowseItem).Assembly }
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

        private static void MoveQuestionnaireEventsToPlainStorage(WriteSideEventStore eventStore)
        {
            var events = eventStore.GetAllEvents();
            var eventsCount = eventStore.CountOfAllEvents();
            
            var countOfScannedEvents = 0;
            foreach (var committedEvent in events)
            {
                var templateImportedEvent = committedEvent.Payload as TemplateImported;
                if (templateImportedEvent != null)
                {
                    plainPostgresTransactionManager.ExecuteInPlainTransaction(() =>
                        StoreQuestionnaireToPlainStorage(templateImportedEvent, committedEvent.EventSourceId,
                            committedEvent.EventSequence));
                }

                var questionnaireDisabledEvent = committedEvent.Payload as QuestionnaireDisabled;
                if (questionnaireDisabledEvent != null)
                {
                    plainPostgresTransactionManager.ExecuteInPlainTransaction(() =>
                    {
                        var questionnaireBrowseItem =
                            questionnaireBrowseItemStorage.AsVersioned()
                                .Get(committedEvent.EventSourceId.FormatGuid(),
                                    questionnaireDisabledEvent.QuestionnaireVersion);
                        if (questionnaireBrowseItem != null)
                        {
                            questionnaireBrowseItem.Disabled = true;
                            questionnaireBrowseItemStorage.AsVersioned()
                                .Store(questionnaireBrowseItem, committedEvent.EventSourceId.FormatGuid(),
                                    questionnaireDisabledEvent.QuestionnaireVersion);
                        }
                    });
                }

                var questionnaireDeletedEvent = committedEvent.Payload as QuestionnaireDeleted;
                if (questionnaireDeletedEvent != null)
                {
                    plainPostgresTransactionManager.ExecuteInPlainTransaction(() =>
                    {
                        var questionnaireBrowseItem =
                            questionnaireBrowseItemStorage.AsVersioned()
                                .Get(committedEvent.EventSourceId.FormatGuid(),
                                    questionnaireDeletedEvent.QuestionnaireVersion);
                        if (questionnaireBrowseItem != null)
                        {
                            questionnaireBrowseItem.IsDeleted = true;
                            questionnaireBrowseItemStorage.AsVersioned()
                                .Store(questionnaireBrowseItem, committedEvent.EventSourceId.FormatGuid(),
                                    questionnaireDeletedEvent.QuestionnaireVersion);
                        }
                    });
                }
                countOfScannedEvents++;

                if (countOfScannedEvents % 5000 == 0)
                {
                    Console.WriteLine(@"Scanned {0} out of {1}", countOfScannedEvents, eventsCount);
                }
            }
        }

        private static void StoreQuestionnaireToPlainStorage(TemplateImported templateImportedEvent,
            Guid questionnaireId, long eventSequence)
        {
            var document = templateImportedEvent.Source;
            var newVersion = templateImportedEvent.Version ?? eventSequence;
            plainQuestionnaireRepository.StoreQuestionnaire(questionnaireId, newVersion, document);
            questionnaireBrowseItemStorage.AsVersioned()
                .Store(
                    new QuestionnaireBrowseItem(document, newVersion, templateImportedEvent.AllowCensusMode,
                        templateImportedEvent.ContentVersion ?? 1), questionnaireId.FormatGuid(), newVersion);

            var questionnaireEntityId = new QuestionnaireIdentity(questionnaireId, newVersion).ToString();

            referenceInfoForLinkedQuestionsStorage.Store(
                referenceInfoForLinkedQuestionsFactory.CreateReferenceInfoForLinkedQuestions(document, newVersion),
                questionnaireEntityId);
            questionnaireExportStructureStorage.Store(
                exportViewFactory.CreateQuestionnaireExportStructure(document, newVersion), questionnaireEntityId);
            questionnaireRosterStructureStorage.Store(
                questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(document, newVersion),
                questionnaireEntityId);
            questionnaireQuestionsInfoStorage.Store(new QuestionnaireQuestionsInfo
            {
                QuestionIdToVariableMap =
                    document.Find<IQuestion>(question => true).ToDictionary(x => x.PublicKey, x => x.StataExportCaption)
            }, questionnaireEntityId);
        }

        private static IReferenceInfoForLinkedQuestionsFactory referenceInfoForLinkedQuestionsFactory =
            new ReferenceInfoForLinkedQuestionsFactory();

        private static IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory = new QuestionnaireRosterStructureFactory();

        private static IExportViewFactory exportViewFactory = new ExportViewFactory(questionnaireRosterStructureFactory, new FileSystemIOAccessor());
        private static PlainPostgresTransactionManager plainPostgresTransactionManager;
        private static IPlainQuestionnaireRepository plainQuestionnaireRepository;
        private static IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage;

        private static IPlainKeyValueStorage<ReferenceInfoForLinkedQuestions> referenceInfoForLinkedQuestionsStorage;
        private static IPlainKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage;
        private static IPlainKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage;
        private static IPlainKeyValueStorage<QuestionnaireQuestionsInfo> questionnaireQuestionsInfoStorage;

        private static EventTypeResolver CreateEventTypeResolver()
        {
            var eventTypeResolver = new EventTypeResolver();
            var eventNamespaces = new[]
            {"WB.Core.SharedKernels.DataCollection.Events", "Main.Core.Events", "WB.Core.Synchronization.Events"};
            var assemlies = new[] { typeof(TabletRegistered).Assembly, typeof(NewUserCreated).Assembly };
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

        private static Dictionary<string, string> GetQuestionnairesFromReadSide(string PGReadSideConnection)
        {
            var questionnaires = new Dictionary<string,string>();

            using (var connection = new NpgsqlConnection(PGReadSideConnection))
            {
                connection.Open();
                using (connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM questionnairedocumentversioneds";
                    using (IDataReader npgsqlDataReader = command.ExecuteReader())
                    {
                        while (npgsqlDataReader.Read())
                        {
                            string value = (string)npgsqlDataReader["value"];
                            string id = (string)npgsqlDataReader["id"];

                            questionnaires.Add(id,value);
                        }
                    }
                }
            }

            return questionnaires;
        }

        private static void InsertItemIfNotExists(string PGPlainConnection, string id, string valueToExtract)
        {
            using (var connection = new NpgsqlConnection(PGPlainConnection))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var checkCommand = connection.CreateCommand();

                    checkCommand.CommandText = $"select exists(select 1 from questionnairedocuments where id=:id)";
                    checkCommand.Parameters.AddWithValue("id", id);

                    var exists = (bool)checkCommand.ExecuteScalar();

                    if (!exists)
                    {
                        var insertCommand = connection.CreateCommand();

                        insertCommand.CommandText = $"INSERT INTO questionnairedocuments VALUES(:id, :value)";

                        //hack do not pull here classes and deserialize full objects
                        var root = JObject.Parse(valueToExtract);
                        var questionnaire = root["Questionnaire"].ToString(Newtonsoft.Json.Formatting.None);

                        if(string.IsNullOrWhiteSpace(questionnaire))
                            throw new Exception("Invalid Questionnaire content.");
                        
                        var valueParameter = new NpgsqlParameter("value", NpgsqlDbType.Json) { Value = questionnaire };
                        var parameter = new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = id };
                        insertCommand.Parameters.Add(parameter);
                        insertCommand.Parameters.Add(valueParameter);
                        var queryResult = insertCommand.ExecuteNonQuery();

                        if (queryResult > 1)
                        {
                            throw new Exception(string.Format("Unexpected row count of deleted records. Expected to delete not more than 1 row, but affected {0} number of rows", queryResult));
                        }

                        transaction.Commit();

                        Console.WriteLine("Questionnaire {0} inserted.", id);
                    }
                    else
                    {
                        Console.WriteLine("Questionnaire {0} exists. Ignoring.", id);
                    }
                }
            }
        }

    }
    internal class Event
    {
        public Guid Id { get; set; }
        public string Origin { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid EventSourceId { get; set; }
        public long GlobalSequence { get; set; }
        public long EventSequence { get; set; }
        public string Value { get; set; }
        public string EventType { get; set; }
    }
}
