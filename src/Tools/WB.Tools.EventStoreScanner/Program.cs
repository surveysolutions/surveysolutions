using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Main.Core.Events.User;
using Moq;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Synchronization.Events.Sync;
using WB.Infrastructure.Native.Storage.EventStore;
using WB.Infrastructure.Native.Storage.EventStore.Implementation;

namespace WB.Tools.EventStoreScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            var eventStoreSettings = GetEventStoreSettingsFromCommandLineArguments(args);
            Console.WriteLine(
                @"Event store settings: serverip: {0}, servertcpport: {1}, serverhttpport: {2}, usebson: {3}, login: {4}, password:{5}",
                eventStoreSettings.ServerIP, eventStoreSettings.ServerTcpPort, eventStoreSettings.ServerHttpPort,
                eventStoreSettings.UseBson, eventStoreSettings.Login, eventStoreSettings.Password);


            Console.WriteLine(@"---------------------------------------------------------------------");
            var eventTypeResolver = CreateEventTypeResolver();

            var eventStore = new WriteSideEventStore(new EventStoreConnectionProvider(eventStoreSettings),
                Mock.Of<ILogger>(), eventStoreSettings, eventTypeResolver);

            ScanEventStoreForUserNameDuplicates(eventStore);

            Console.WriteLine(@"Done");
            Console.ReadLine();
        }

        private static void ScanEventStoreForUserNameDuplicates(WriteSideEventStore eventStore)
        {
            var events = eventStore.GetAllEvents();
            var eventsCount = eventStore.CountOfAllEvents();

            var userNames = new Dictionary<string, List<Guid>>();
            var countOfScannedEvents = 0;
            foreach (var committedEvent in events)
            {
                var createUserEvent = committedEvent.Payload as NewUserCreated;
                if (createUserEvent != null)
                {
                    var userNameLowerCase = createUserEvent.Name.ToLower();
                    if (!userNames.ContainsKey(userNameLowerCase))
                        userNames[userNameLowerCase] = new List<Guid>();
                    userNames[userNameLowerCase].Add(committedEvent.EventSourceId);
                }
                countOfScannedEvents++;

                if (countOfScannedEvents%5000 == 0)
                {
                    Console.WriteLine(@"Scanned {0} out of {1}", countOfScannedEvents, eventsCount);
                }
            }

            foreach (var userName in userNames)
            {
                if (userName.Value.Count > 1)
                {
                    Console.WriteLine(@"User name '{0}' is duplicated by event sources:{1}", userName.Key,
                        string.Join(",", userName.Value));
                    Console.WriteLine(@"---------------------------------------------------------------------");
                }
            }
        }

        private static EventTypeResolver CreateEventTypeResolver()
        {
            var eventTypeResolver =new EventTypeResolver();
            var eventNamespaces = new[]
            {"WB.Core.SharedKernels.DataCollection.Events", "Main.Core.Events", "WB.Core.Synchronization.Events"};
            var assemlies = new[] {typeof (TabletRegistered).Assembly, typeof (NewUserCreated).Assembly};
            var events =
                assemlies.SelectMany(a=>a.GetTypes())
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

        private static EventStoreSettings GetEventStoreSettingsFromCommandLineArguments(string[] args)
        {
            var eventStoreSettings = new EventStoreSettings()
            {
                ServerIP = "127.0.0.1",
                ServerTcpPort = 1113,
                ServerHttpPort = 2113,
                Login = "admin",
                Password = "changeit",
                UseBson = false
            };

            for (int i = 0; i < args.Length-1; i++)
            {
                if (args[i][0] != '-')
                    continue;

                var propertyName = args[i].Substring(1).ToLower();
                switch (propertyName)
                {
                    case "serverip":
                        eventStoreSettings.ServerIP = args[i + 1];
                        break;
                    case "login":
                        eventStoreSettings.Login = args[i + 1];
                        break;
                    case "password":
                        eventStoreSettings.Password = args[i + 1];
                        break;
                    case "servertcpport":
                        eventStoreSettings.ServerTcpPort = int.Parse(args[i + 1]);
                        break;
                    case "serverhttpport":
                        eventStoreSettings.ServerHttpPort = int.Parse(args[i + 1]);
                        break;
                    case "usebson":
                        eventStoreSettings.UseBson = bool.Parse(args[i + 1]);
                        break;
                }
            }
            return eventStoreSettings;
        }
    }
}
