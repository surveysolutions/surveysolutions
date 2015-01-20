using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Embedded;
using EventStore.Core;
using EventStore.Core.Bus;
using EventStore.Core.Messages;
using Machine.Specifications;
using Moq;
using Ncqrs;
using Raven.Client.Linq;
using WB.Core.Infrastructure.Storage.EventStore;

namespace WB.Tests.Integration.EventStore
{
    public class with_in_memory_event_store
    {
        protected static IEventStoreConnectionProvider ConnectionProvider;
        private static ClusterVNode node;

        Establish context = () =>
        {
            NcqrsEnvironment.InitDefaults();

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
            catch (System.TypeInitializationException exception)
            {
                Console.WriteLine(exception.Message);

                ReflectionTypeLoadException innerException = (ReflectionTypeLoadException)exception.InnerException;

                throw new ApplicationException(string.Format("LoaderMessages: {1} ", innerException.Message, string.Join(",", innerException.LoaderExceptions.Select(x => x.Message))));
            }

            var startedEvent = new ManualResetEventSlim(false);
            node.MainBus.Subscribe(
                new AdHocHandler<UserManagementMessage.UserManagementServiceInitialized>(m => startedEvent.Set()));

            node.Start();

            if (!startedEvent.Wait(60000))
                throw new TimeoutException("Test EventStore node haven't started in 60 seconds.");

            ConnectionProvider = Mock.Of<IEventStoreConnectionProvider>(x => x.Open() == EmbeddedEventStoreConnection.Create(node, null));
        };

        Cleanup staff = () => node.Stop();
    }
}