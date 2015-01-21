using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Main.Core.Events;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Nito.AsyncEx.Synchronous;
using Polly;
using Raven.Client;
using Raven.Client.Document;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.Storage.EventStore;
using WB.Core.Infrastructure.Storage.EventStore.Implementation;
using WB.Core.Infrastructure.Storage.Raven;
using WB.Core.Infrastructure.Storage.Raven.Implementation;
using WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveyManagement;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;

namespace WB.Tools.EventsMigrator
{
    public class Executor
    {
        private string status;
        private int totalEvents;
        private int processed;
        private TimeSpan elapsed;

        public void Process(IShell settings, CancellationToken token)
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);

            var ravenStore = GetRavenStore(settings);
            var eventStoreConnectionProvider = new EventStoreConnectionProvider(new EventStoreConnectionSettings
            {
                ServerIP = settings.EventStoreIP,
                ServerHttpPort = settings.EventStoreHttpPort,
                ServerTcpPort = settings.EventStoreTcpPort,
                Login = settings.EventStoreLogin,
                Password = settings.EventStorePassword
            });

            this.status = "Counting number of events to process";
            this.totalEvents = ravenStore.CountOfAllEvents();

            this.processed = settings.SkipEvents;

            RegisterEvents(settings);

            Stopwatch watch = Stopwatch.StartNew();

            this.status = "Getting events from event store";
            int processedInCurrentRun = 0;
            var policy = Policy.Handle<ObjectDisposedException>()
                .Or<IOException>()
                .Or<HttpRequestException>()
                .Or<TimeoutException>()
                   .WaitAndRetryAsync(settings.RetryTimes, retryAttempt => TimeSpan.FromSeconds(5),
                       (exception, duration) =>
                           Caliburn.Micro.Execute.OnUIThread(() =>
                               settings.ErrorMessages.Add(string.Format("Exception '{2}' cought. Message: '{0}'. Waiting for {1} seconds to retry",
                                   exception.Message, duration.TotalSeconds, exception.GetType()))));
            try
            {
                policy.ExecuteAsync(async () =>
                {
                    using (var connection = eventStoreConnectionProvider.Open())
                    {
                        await connection.ConnectAsync();

                        int skipEvents = Math.Max(0, settings.SkipEvents + processedInCurrentRun - 1);
                        foreach (CommittedEvent[] @event in ravenStore.GetAllEvents(50, skipEvents))
                        {
                            foreach (var committedEvent in @event)
                            {
                                token.ThrowIfCancellationRequested();
                                int expectedVersion = ((int)committedEvent.EventSequence - 2);
                                string stream = WriteSideEventStore.EventsPrefix + committedEvent.EventSourceId.FormatGuid();

                                var eventData = WriteSideEventStore.BuildEventData(new UncommittedEvent(committedEvent.EventIdentifier,
                                    committedEvent.EventSourceId,
                                    committedEvent.EventSequence,
                                    0,
                                    committedEvent.EventTimeStamp,
                                    committedEvent.Payload));

                                await connection.AppendToStreamAsync(stream, expectedVersion, eventData);

                                processed++;
                                processedInCurrentRun++;
                                this.elapsed = watch.Elapsed;
                            }
                        }
                    }
                }).WaitAndUnwrapException(token);
            }
            finally
            {
                processedInCurrentRun = 0;
            }

            this.status = "Done writing events";
            watch.Stop();
        }

        private static RavenDBEventStore GetRavenStore(IShell settings)
        {
            var ravenConnectionSettings = new RavenConnectionSettings(
                   settings.ServerAddress,
                   eventsDatabase: settings.RavenDatabaseName);

            var instance = new DocumentStoreProvider(ravenConnectionSettings);
            IDocumentStore separateInstanceForEventStore = instance.CreateSeparateInstanceForEventStore();
            var store = new RavenDBEventStore(separateInstanceForEventStore, 1000, useStreamingForAllEvents: true);
            return store;
        }

        private static void RegisterEvents(IShell settings)
        {
            var assemblies = new List<Assembly> { typeof(AggregateRootEvent).Assembly };

            if (settings.SelectedAppName == "Designer")
            {
                assemblies.Add(typeof(DesignerBoundedContextModule).Assembly);
                assemblies.Add(typeof(UserLoggedIn).Assembly);
            }
            else
            {
                assemblies.Add(typeof(DataCollectionSharedKernelModule).Assembly);
                assemblies.Add(typeof(IAuthentication).Assembly);
                assemblies.Add(typeof(SurveyManagementSharedKernelModule).Assembly);
            }

            var types = GetAllEventTypes(assemblies).ToList();
            types.ForEach(NcqrsEnvironment.RegisterEventDataType);
        }

        private static IEnumerable<Type> GetAllEventTypes(IEnumerable<Assembly> assemblies)
        {
            return (from assembly in assemblies
                    from type in assembly.GetTypes()
                    where IsEventHandler(type)
                    from handledEventType in GetHandledEventTypes(type)
                    select handledEventType).Distinct();
        }

        private static IEnumerable<Type> GetHandledEventTypes(Type type)
        {
            foreach (var handlerInterfaceType in type.GetInterfaces().Where(IsIEventHandlerInterface))
            {
                var eventDataType = handlerInterfaceType.GetGenericArguments().First();
                yield return eventDataType;
            }
        }

        private static bool IsIEventHandlerInterface(Type type)
        {
            var isIEventHandlerInterface = type.IsInterface &&
                type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(IEventHandler<>);
            return isIEventHandlerInterface;
        }

        private static bool IsEventHandler(Type type)
        {
            return type.GetInterfaces().Any(x =>
                                      x.IsGenericType &&
                                      x.GetGenericTypeDefinition() == typeof(IEventHandler<>));
        }


        public ExecutionStatus GetStatus()
        {
            return new ExecutionStatus
            {
                Status = this.status,
                Total = this.totalEvents,
                Elapsed = this.elapsed,
                Processed = this.processed,
            };
        }
    }

    public class ExecutionStatus
    {
        private TimeSpan elapsed;
        public string Status { get; set; }

        public int Processed { get; set; }

        public int Total { get; set; }

        public TimeSpan Elapsed
        {
            get { return elapsed; }
            set { elapsed = value; }
        }
    }
}