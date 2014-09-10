using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Raven.Abstractions.Data;
using Raven.Abstractions.Json;
using Raven.Abstractions.Smuggler;
using Raven.Client;
using Raven.Client.Connection;
using Raven.Client.Document;
using Raven.Client.Extensions;
using Raven.Client.Indexes;
using Raven.Imports.Newtonsoft.Json;
using Raven.Json.Linq;
using Raven.Smuggler;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide;
using WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide.Indexes;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using StoredEvent = WB.Core.Infrastructure.Storage.Raven.StoredEvent;

namespace WB.Tools.DatabaseCleaner
{
    class Program
    {
        private static IQueryableReadSideRepositoryReader<InterviewSummary> interviews;
        private static IQueryableReadSideRepositoryReader<HardDeletedInterview> hardDeletedInterviewStorage;
        private static DocumentStore sourceStorage;
        static void Main(string[] args)
        {
            try
            {
                var settings = new CommandLineSettings(args);

                if (string.IsNullOrEmpty(settings.Url) ||
                    string.IsNullOrEmpty(settings.SoureViewStorageDbName))
                {
                    Console.WriteLine("Usage:");
                    Console.WriteLine("- Clear eventstore based on readside");
                    Console.WriteLine(string.Format("WB.Tools.DatabaseCleaner -url http://localhost:8080 -events Events -views Views -statuses ApprovedByHeadquarters,Deleted -website http://localhost/Supervisor"));
                    Console.WriteLine("- You could skip -source paramater if evenets stored in System database");
                    Console.WriteLine("- You could skip -statuses paramater then interviews with statuses ApprovedByHeadquarters and Deleted will be cleaned");
                    Console.WriteLine("- You could skip -website paramater if don't want to run rebuild readside immediately");
                    return;
                }
                sourceStorage = CreateServerStorage(settings.Url, settings.SoureStorageDbName, CreateStoreConventions(CollectionName));

                CreateTemporaryDbForDeletedEvents(settings.Url, settings.BackupStorageDbName);

                var backupStorage = CreateServerStorage(settings.Url, settings.BackupStorageDbName, CreateStoreConventions(CollectionName));

                interviews = CreateInterviewReadSide<InterviewSummary>(settings.Url, settings.SoureViewStorageDbName);
                hardDeletedInterviewStorage = CreateInterviewReadSide<HardDeletedInterview>(settings.Url, settings.SoureViewStorageDbName);

                var interviewsForDelete = GetInterviewsForDelete(settings);
                interviewsForDelete.AddRange(GetHardInterviews());

                RegisterEvents();

                for (int i = 0; i < interviewsForDelete.Count; i++)
                {
                    var handledPersent = (int)((((decimal)i / interviewsForDelete.Count)) * 100);
                    Console.WriteLine(string.Format("handled {0}%", handledPersent));
                
                    var interviewId = interviewsForDelete[i];
                    var allEventsByInterview = GetEventStreamByInterview(interviewId);

                    if (!allEventsByInterview.Any())
                        continue;

                    SaveEventsByInterviewInTemporaryDataBase(backupStorage, allEventsByInterview);

                    Clear(interviewId);
                }
                
                BackupTemporaryDataBase(backupStorage, settings.Stauses);
                DeleteTemporaryDataBase(backupStorage);
                RunRebuildReadSide(settings.WebsiteUrl);
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            Console.ReadLine();
        }

        private static void RunRebuildReadSide(string websiteUrl)
        {
            if (string.IsNullOrEmpty(websiteUrl))
            {
                Console.WriteLine("You must rebuild readside manually");
                return;
            }

            Console.WriteLine("Starting rebuild readside");
            WebRequest webRequest = WebRequest.Create(websiteUrl + "/ControlPanel/RebuildReadSide");
            
            webRequest.GetResponse();

            Process.Start(websiteUrl + "/ControlPanel/ReadSide");
        }

        private static List<Guid> GetInterviewsForDelete(CommandLineSettings settings)
        {
            var interviewsForDelete = new List<InterviewSummary>();
            foreach (var interviewStatuse in settings.Stauses)
            {
                interviewsForDelete.AddRange(
                    interviews.QueryAll((i) => i.Status == interviewStatuse));
            }
            return interviewsForDelete.Select(i=>i.InterviewId).ToList();
        }


        private static List<Guid> GetHardInterviews()
        {
            return hardDeletedInterviewStorage.QueryAll(null).ToArray().Select(q => q.InterviewId).ToList();
        }

        private static void DeleteTemporaryDataBase(DocumentStore backupStorage)
        {
            Console.WriteLine("Deleting temporary db");
            while (DocumentsInViewsDatabaseCount(backupStorage) > 0)
            {
                backupStorage.DatabaseCommands.DeleteByIndex("Raven/DocumentsByEntityName", new IndexQuery(), false);
                Thread.Sleep(3000);
            }
            Console.WriteLine("Temporary db is deleted");
        }

        private static int DocumentsInViewsDatabaseCount(DocumentStore ravenStore)
        {
            int resultViewCount = 0;
            using (IDocumentSession session = ravenStore.OpenSession())
            {
                resultViewCount = session
                    .Query<object>("Raven/DocumentsByEntityName")
                    .Customize(customization => customization.WaitForNonStaleResultsAsOfNow())
                    .Count();
            }
            return resultViewCount;
        }

        private static void SaveEventsByInterviewInTemporaryDataBase(DocumentStore backupStorage, List<StoredEvent> allEventsByInterview)
        {
            using (IDocumentSession session = backupStorage.OpenSession())
            {
                foreach (var storedEvent in allEventsByInterview)
                {
                    session.Store(storedEvent);
                }
                session.SaveChanges();
            }
        }

        private static void CreateTemporaryDbForDeletedEvents(string sourceStorageUrl, string backupStorageDbName)
        {
            IDocumentStore documentStore = new DocumentStore { Url = sourceStorageUrl };
            documentStore.Initialize();
            documentStore.DatabaseCommands.EnsureDatabaseExists(backupStorageDbName);
        }

        private static void BackupTemporaryDataBase(DocumentStore backupStorage, InterviewStatus[] statuses)
        {
            Console.WriteLine("Creating backup of cleaned events");
            var smugglerOptions = new SmugglerOptions { };

            var connectionStringOptions = new RavenConnectionStringOptions
            {
                ApiKey = backupStorage.ApiKey,
                Credentials = backupStorage.Credentials,
                DefaultDatabase = backupStorage.DefaultDatabase,
                Url = backupStorage.Url,
            };
            var orignFileName = string.Format("cleared_{0}{1}", string.Concat(statuses), DateTime.Now.ToLongDateString());
            var backupFileName = orignFileName+".ravendump";
            int attempt = 0;
            while (File.Exists(backupFileName))
            {
                attempt++;
                backupFileName = string.Format("{0}{1}.ravendump", orignFileName, attempt);
            }
            var smugglerApi = new SmugglerApi(smugglerOptions, connectionStringOptions);

            var task = TaskEx.Run(async () => await
                smugglerApi.ExportData(null,
                    new SmugglerOptions
                    {
                        BackupPath = backupFileName,
                        OperateOnTypes = ItemType.Documents
                    }, false));
            
            task.Wait();
            
            Console.WriteLine("Backup is {0}", backupFileName);
        }
        private static IQueryableReadSideRepositoryReader<T> CreateInterviewReadSide<T>(string url, string databaseName) where T : class, IReadSideRepositoryEntity
        {
            return new RavenReadSideRepositoryReader<T>(CreateServerStorage(url, databaseName),
                Mock.Of<IReadSideStatusService>(_ => _.AreViewsBeingRebuiltNow() == false));
        }

        private const string CollectionName = "Events";

        private static DocumentStore CreateServerStorage(string url, string databaseName, DocumentConvention convention=null)
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
        private static List<StoredEvent> GetEventStreamByInterview(Guid eventSourceId)
        {
            int pageSize = 1024;
            var result = new List<StoredEvent>();
            int page = 0;
            while (true)
            {
                using (IDocumentSession session = sourceStorage.OpenSession())
                {
                    List<StoredEvent> chunk = session
                        .Query<StoredEvent, Event_ByEventSource>()
                        .Customize(x => x.WaitForNonStaleResults())
                        .Where(x => x.EventSourceId == eventSourceId).OrderBy(e => e.EventSequence)
                        .Skip(page * pageSize)
                        .Take(pageSize)
                        .ToList();

                    if (chunk.Count == 0)
                    {
                        break;
                    }

                    result.AddRange(chunk);
                    page++;
                }
            }

            return result;
        }



        private static void Clear(Guid eventSourceId)
        {
            while (HasDocs(eventSourceId))
            {
                sourceStorage.DatabaseCommands.DeleteByIndex("Event/ByEventSource", new IndexQuery
                {
                    Query = string.Format("EventSourceId:{0}", eventSourceId)
                }, allowStale: false);
            }
        }

        private static bool HasDocs(Guid eventSourceId)
        {
            using (var session = sourceStorage.OpenSession())
            {
                var any = session.Query<StoredEvent, Event_ByEventSource>()
                        .Customize(q => q.WaitForNonStaleResultsAsOfNow()).Where(s => s.EventSourceId == eventSourceId)
                        .Any();

                return any;
            }
        }

        private static void RegisterEvents()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
            string namespaceDataCollection = "WB.Core.SharedKernels.DataCollection.Events";
            string namespaceMainCore = "Main.Core.Events";

            var typesInAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t => t.GetTypes());
            var q = from t in typesInAssembly
                    where t.IsClass && !t.IsAbstract && t.Namespace != null && (t.Namespace.Contains(namespaceDataCollection) || t.Namespace.Contains(namespaceMainCore))
                    select t;
            q.ToList().ForEach(NcqrsEnvironment.RegisterEventDataType);
        }

        private class CommandLineSettings
        {
            public CommandLineSettings(string[] args)
            {
                Stauses = new InterviewStatus[] { InterviewStatus.Deleted, InterviewStatus.ApprovedByHeadquarters };

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-url")
                    {
                        Url = args[i + 1];
                        continue;
                    }
                    if (args[i] == "-events")
                    {
                        SoureStorageDbName = args[i + 1];
                        continue;
                    }
                    if (args[i] == "-views")
                    {
                        SoureViewStorageDbName = args[i + 1];
                        continue;
                    }
                    if (args[i] == "-statuses")
                    {
                        var notParsedStatuses = args[i + 1];
                        var statusesStringArray = notParsedStatuses.Split(',');
                        var parsedStatuses = new List<InterviewStatus>();
                        foreach (var statusString in statusesStringArray)
                        {
                            InterviewStatus status;
                            if (InterviewStatus.TryParse(statusString, true, out status))
                            {
                                parsedStatuses.Add(status);
                            }
                        }
                        Stauses = parsedStatuses.ToArray();
                        continue;
                    }
                    if (args[i] == "-website")
                    {
                        WebsiteUrl = args[i + 1];
                    }
                }
            }

            public string Url { get; private set; }
            public string WebsiteUrl { get; private set; }
            public string SoureStorageDbName { get; private set; }
            public string SoureViewStorageDbName { get; private set; }
            public InterviewStatus[] Stauses { get; private set; }

            public string BackupStorageDbName
            {
                get { return SoureViewStorageDbName + "_tmp"; }
            }
        }
    }
}
