using System;
using System.Net;
using System.Threading;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using EventStore.ClientAPI.SystemData;
using EventStore.Core;
using EventStore.Core.Bus;
using EventStore.Core.Messages;
using Machine.Specifications;
using Moq;
using Ncqrs;
using WB.Core.Infrastructure.EventBus;
using WB.Infrastructure.Native.Storage.EventStore;

namespace WB.Tests.Integration.EventStoreTests
{
    public class with_in_memory_event_store
    {
        protected class AccountRegistered : IEvent
        {
            public string ApplicationName { get; set; }
            public string ConfirmationToken { get; set; }
            public string Email { get; set; }
        }

        protected class AccountConfirmed : IEvent { }

        protected class AccountLocked : IEvent { }

        protected static IEventStoreConnectionProvider ConnectionProvider;
        private static ClusterVNode node;

        Establish context = () =>
        {
            var emptyEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            try
            {
                node = EmbeddedVNodeBuilder.AsSingleNode()
                    .RunInMemory()
                    .WithInternalHttpOn(emptyEndpoint)
                    .WithInternalTcpOn(emptyEndpoint)
                    .WithExternalHttpOn(emptyEndpoint)
                    .WithExternalTcpOn(emptyEndpoint)
                    .Build();
            }
            catch (TypeInitializationException )
            {
                node = EmbeddedVNodeBuilder.AsSingleNode()
                   .RunInMemory()
                   .WithInternalHttpOn(emptyEndpoint)
                   .WithInternalTcpOn(emptyEndpoint)
                   .WithExternalHttpOn(emptyEndpoint)
                   .WithExternalTcpOn(emptyEndpoint)
                   .Build();

                //Console.WriteLine(exception.Message);

                //ReflectionTypeLoadException innerException = (ReflectionTypeLoadException)exception.InnerException;
                //var first = innerException.LoaderExceptions.First();
                //Console.WriteLine("First stack trace:" + first.StackTrace);


                //throw new ApplicationException(
                //    string.Format("LoaderMessages: {1} ", innerException.Message, string.Join(",", innerException.LoaderExceptions.Select(x => x.Message))), exception);

            }

            var startedEvent = new ManualResetEventSlim(false);
            node.MainBus.Subscribe(
                new AdHocHandler<UserManagementMessage.UserManagementServiceInitialized>(m => startedEvent.Set()));

            node.Start();

            if (!startedEvent.Wait(60000))
                throw new TimeoutException("Test EventStore node haven't started in 60 seconds.");

            var settings = ConnectionSettings.Create()
                .UseConsoleLogger()
                .EnableVerboseLogging()
                .SetDefaultUserCredentials(new UserCredentials("admin", "changeit"))
                .KeepReconnecting();
            ConnectionProvider = Mock.Of<IEventStoreConnectionProvider>(x => x.Open() == EmbeddedEventStoreConnection.Create(node, settings, null));
        };

        Cleanup staff = () => node.Stop();
    }
}