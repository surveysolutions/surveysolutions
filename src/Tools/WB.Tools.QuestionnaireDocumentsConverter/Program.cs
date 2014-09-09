using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Raven.Abstractions.Json;
using Raven.Client.Document;
using Raven.Imports.Newtonsoft.Json;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide;
using WB.Core.SharedKernels.QuestionnaireUpgrader.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireUpgrader.Services;

namespace WB.Tools.QuestionnaireDocumentsConverter
{
    class Program
    {
        private static IEventStore eventStore;
        private static IQuestionnaireUpgradeService questionnaireUpgradeService;
        private const string CollectionName = "Events";

        static void Main(string[] args)
        {
            try
            {
                var settings = new CommandLineSettings(args);

                if (string.IsNullOrEmpty(settings.DataBaseUrl) ||
                    string.IsNullOrEmpty(settings.ViewStoreDbName))
                {
                    Console.WriteLine("Usage:");
                    Console.WriteLine("- Create variable names for rosters");
                    Console.WriteLine(string.Format("WB.Tools.QuestionnaireDocumentsConverter -url http://localhost:8080 -events Events -views Views"));
                    Console.WriteLine("- You could skip -events paramater if evenets stored in System database");
                    return;
                }
                ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
                questionnaireUpgradeService = new QuestionnaireUpgradeService(new FileSystemIOAccessor());

                eventStore =
                    new RavenDBEventStore(
                        CreateServerStorage(settings.DataBaseUrl, settings.EventStoreDbName, CreateStoreConventions(CollectionName)), 1024);
                RegisterEvents();

                var questionnaireStorage =
                    new RavenReadSideRepositoryReader<QuestionnaireDocument>(
                        CreateServerStorage(settings.DataBaseUrl, settings.ViewStoreDbName),
                        Mock.Of<IReadSideStatusService>(_ => _.AreViewsBeingRebuiltNow() == false));

                Console.WriteLine("querying for QuestionnaireDocuments");
                var questionnaires = questionnaireStorage.QueryAll(i => !i.IsDeleted).ToList();
                int processed = 0;
                foreach (var questionnaireDocument in questionnaires)
                {
                    var handledPersent = (int)((((decimal)processed / questionnaires.Count)) * 100);
                    Console.WriteLine(string.Format("handled {0}%", handledPersent));
                    var eventStreamByQuestionnaire = eventStore.ReadFrom(questionnaireDocument.PublicKey, 0, long.MaxValue);

                    if(eventStreamByQuestionnaire.IsEmpty)
                        continue;

                    var lastEvent = eventStreamByQuestionnaire.Last();
                    var listOfNewRostersVariables = questionnaireUpgradeService.GetMissingVariableNames(questionnaireDocument);
                    var uncommittedEventStream = new UncommittedEventStream(Guid.NewGuid(), "update-roster-variables");

                    var sequence = lastEvent.EventSequence + 1;
                    foreach (var listOfNewRostersVariable in listOfNewRostersVariables)
                    {
                        uncommittedEventStream.Append(new UncommittedEvent(Guid.NewGuid(), questionnaireDocument.PublicKey,
                            sequence, 1,
                            DateTime.Now, new GroupUpdated
                            {
                                GroupPublicKey = listOfNewRostersVariable.Value.PublicKey,
                                GroupText = listOfNewRostersVariable.Value.Title,
                                VariableName = listOfNewRostersVariable.Key,
                                Description = listOfNewRostersVariable.Value.Description,
                                ConditionExpression = listOfNewRostersVariable.Value.ConditionExpression,
                                ResponsibleId = questionnaireDocument.CreatedBy ?? Guid.Empty
                            }, lastEvent.EventVersion));
                        sequence++;
                    }

                    if (uncommittedEventStream.Any())
                        eventStore.Store(uncommittedEventStream);
                    processed++;
                }
            } 

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        private static DocumentStore CreateServerStorage(string url, string databaseName, DocumentConvention convention = null)
        {
            var store = new DocumentStore
            {
                Url = url,
                Conventions = { JsonContractResolver = new PropertiesOnlyContractResolver() },
                DefaultDatabase = databaseName
            };

            if (convention != null)
                store.Conventions = convention;

            store.Initialize();

            return store;
        }

        protected static DocumentConvention CreateStoreConventions(string ravenCollectionName)
        {
            return new DocumentConvention
            {
                JsonContractResolver = new PropertiesOnlyContractResolver(),
                FindTypeTagName = x => ravenCollectionName,
                CustomizeJsonSerializer = CustomizeJsonSerializer,
            };
        }

        private static void CustomizeJsonSerializer(JsonSerializer serializer)
        {
            SetupSerializerToIgnoreAssemblyNameForEvents(serializer);
        }

        private static void SetupSerializerToIgnoreAssemblyNameForEvents(JsonSerializer serializer)
        {
            serializer.Binder = new IgnoreAssemblyNameForEventsSerializationBinder();

            // if we want to perform serialized type name substitution
            // then JsonDynamicConverter should be removed
            // that is because JsonDynamicConverter handles System.Object types
            // and it by itself does not recognized substituted type
            // and does not allow our custom serialization binder to work
            RemoveJsonDynamicConverter(serializer.Converters);
        }

        private static void RemoveJsonDynamicConverter(JsonConverterCollection converters)
        {
            JsonConverter jsonDynamicConverter = converters.SingleOrDefault(converter => converter is JsonDynamicConverter);

            if (jsonDynamicConverter != null)
            {
                converters.Remove(jsonDynamicConverter);
            }
        }



        private static void RegisterEvents()
        {
            string namespaceMainCore = "Main.Core.Events.Questionnaire";
            string namespaceDesigner = "WB.Core.BoundedContexts.Designer.Events.Questionnaire";
            var typesInAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t => t.GetTypes());
            var q = from t in typesInAssembly
                    where t.IsClass && !t.IsAbstract && t.Namespace != null && (t.Namespace.Contains(namespaceMainCore) || t.Namespace.Contains(namespaceDesigner))
                    select t;
            q.ToList().ForEach(NcqrsEnvironment.RegisterEventDataType);
        }
    }
}
