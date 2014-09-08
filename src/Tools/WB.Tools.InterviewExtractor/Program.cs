using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Events.Questionnaire;
using Main.Core.Events.User;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Raven.Client.Document;
using Raven.Client.Extensions;
using Raven.Imports.Newtonsoft.Json;
using WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide;

namespace WB.Tools.InterviewExtractor
{
    class Program
    {
        private static IStreamableEventStore eventStoreSource;
        private static IStreamableEventStore eventStoreTarget;
        private const string system = "<system>";
        static void Main(string[] args)
        {

            int skipEventsCount = args.Length > 5 ? int.Parse(args[5]) : 2;

            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
            var sourceDbUrl = "http://localhost:8081/";//args[0];
            var sourceDatabaseName = "UgandaEvents";//  args[1];
            var targetDbUrl = "http://localhost:8081/";//args[2];
            var targetDatabaseName = "FilteredEvents"; //args[3];
            Guid eventSourseId = Guid.Parse("3a86396f-16eb-4ffb-9a5d-718347d2c4ef" /*args[4]*/);

            eventStoreSource = new RavenDBEventStore(CreateServerStorage(sourceDbUrl, sourceDatabaseName == system ? "" : sourceDatabaseName), 1024);
            eventStoreTarget = new RavenDBEventStore(CreateServerStorage(targetDbUrl, targetDatabaseName), 1024);

            RegisterEvents();

            Guid? commitId = null;

            var eventNumber = skipEventsCount + 1;
            try
            {
                foreach (CommittedEvent[] eventBulk in eventStoreSource.GetAllEvents(skipEvents: skipEventsCount))
                {
                    commitId = Guid.NewGuid();
                    var streamToSave = new UncommittedEventStream(commitId.Value, origin: null);

                    foreach (CommittedEvent @event in eventBulk)
                    {
                        if (@event.EventSourceId == eventSourseId || (@event.Payload is TemplateImported) || (@event.Payload is NewUserCreated))
                        {
                            var eventFromStream = new UncommittedEvent(@event.EventIdentifier, @event.EventSourceId, @event.EventSequence, 1, @event.EventTimeStamp, @event.Payload, @event.EventVersion);
                            streamToSave.Append(eventFromStream);
                        }

                        eventNumber++;
                    }
                    if (streamToSave.Any())
                    {
                        eventStoreTarget.Store(streamToSave);
                    }
                    Console.WriteLine(string.Format("last succefully stored event with sequence {0}.", eventNumber));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(string.Format("last succefully stored event with sequence {0}.", eventNumber));
                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.StackTrace);
            }

            Console.ReadLine();
        }

        private static void RegisterEvents()
        {
            string namespaceDataCollection = "WB.Core.SharedKernels.DataCollection.Events";
            string namespaceMainCore = "Main.Core.Events";

            var typesInAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t => t.GetTypes());
            var q = from t in typesInAssembly
                    where t.IsClass && !t.IsAbstract && t.Namespace != null && (t.Namespace.Contains(namespaceDataCollection) || t.Namespace.Contains(namespaceMainCore))
                    select t;
            q.ToList().ForEach(NcqrsEnvironment.RegisterEventDataType);
        }

        private static DocumentStore CreateServerStorage(string url, string database)
        {
            var store = new DocumentStore
            {
                Url = url,
                DefaultDatabase = database,
                Conventions =
                {
                    JsonContractResolver = new PropertiesOnlyContractResolver(),
                    CustomizeJsonSerializer = serializer =>
                    {
                        serializer.TypeNameHandling = TypeNameHandling.All;
                    }
                }

            };
            store.Initialize();
            if (!string.IsNullOrWhiteSpace(database))
            {
                store.DatabaseCommands.EnsureDatabaseExists(database);
            }
            return store;
        }
    }
}
