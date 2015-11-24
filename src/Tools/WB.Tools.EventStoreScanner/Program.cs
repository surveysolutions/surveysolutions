using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Main.Core.Events.User;
using Moq;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Storage.EventStore;
using WB.Core.Infrastructure.Storage.EventStore.Implementation;
using WB.Core.Synchronization.Events.Sync;

namespace WB.Tools.EventStoreScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            var eventStoreSettings= GetEventStoreSettingsFromCommandLineArguments(args);
            var eventTypeResolver = CreateEventTypeResolver();
            
            var eventStore = new WriteSideEventStore(new EventStoreConnectionProvider(eventStoreSettings),
                Mock.Of<ILogger>(), eventStoreSettings, eventTypeResolver);

            var events = eventStore.GetAllEvents();

            var userNames = new Dictionary<string, List<Guid>>();

            foreach (var committedEvent in events)
            {
                var createUserEvent = committedEvent.Payload as NewUserCreated;
                if (createUserEvent != null)
                {
                    var userNameLowerCase = createUserEvent.Name.ToLower();
                    if(!userNames.ContainsKey(userNameLowerCase))
                        userNames[userNameLowerCase]=new List<Guid>();
                    userNames[userNameLowerCase].Add(committedEvent.EventSourceId);
                }
            }

            foreach (var userName in userNames)
            {
                if (userName.Value.Count > 1)
                {
                    Console.WriteLine(@"User name '{0}' is duplicated by event sources:{1}", userName.Key, string.Join(",", userName.Value));
                }
            }
            Console.ReadLine();
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
