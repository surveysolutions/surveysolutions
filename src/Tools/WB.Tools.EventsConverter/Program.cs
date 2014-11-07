using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Raven.Client.Document;
using WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Tools.EventsConverter
{
    class Program
    {
        private static IStreamableEventStore eventStoreSource;
        private static IStreamableEventStore eventStoreTarget;

        static void Main(string[] args)
        {
            int skipEventsCount = args.Length > 2 ? int.Parse(args[2]) : 0;

            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);

            eventStoreSource = new RavenDBEventStore(CreateServerStorage(args[0]), 1024);
            eventStoreTarget = new RavenDBEventStore(CreateServerStorage(args[1]), 1024);
            var eventSequences = new Dictionary<Guid, long>();
            var eventsFromSingleCommit = new List<CommittedEvent>();
            Guid? commitId = null;
            RegisterEvents();

            var eventNumber = skipEventsCount + 1;
            try
            {
                foreach (CommittedEvent[] eventBulk in eventStoreSource.GetAllEvents(skipEvents: skipEventsCount))
                {
                    foreach (CommittedEvent @event in eventBulk)
                    {
                        if (@event.CommitId != commitId)
                        {
                            if (commitId.HasValue && eventsFromSingleCommit.Any())
                            {
                                var lastEventToStore = eventsFromSingleCommit.Last();
                                var eventSourceId = eventsFromSingleCommit.First().EventSourceId;
                                var sequence = eventSequences.ContainsKey(eventSourceId) ? eventSequences[eventSourceId] : 0;

                                if (sequence == 0 && skipEventsCount > 0)
                                {
                                    var lastEvents = eventStoreTarget.ReadFrom(eventSourceId, 0, long.MaxValue);
                                    if (lastEvents.Any())
                                    {
                                        sequence = lastEvents.Last().EventSequence;
                                    }
                                }

                                var newSequence = ProcessEventsFromCommit(eventsFromSingleCommit, sequence, commitId.Value);
                                eventSequences[eventSourceId] = newSequence;
                                eventsFromSingleCommit = new List<CommittedEvent>();

                                Console.WriteLine(string.Format("last succefully stored event is {0}, with sequence {1}.", lastEventToStore.EventIdentifier,
                                    eventNumber));
                            }
                            commitId = @event.CommitId;
                        }
                        eventsFromSingleCommit.Add(@event);
                        eventNumber++;

                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.StackTrace);
            }
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

        private static DocumentStore CreateServerStorage(string url)
        {
            var store = new DocumentStore
            {
                Url = url,
                Conventions = { JsonContractResolver = new PropertiesOnlyContractResolver() }
            };

            /*   if (!string.IsNullOrWhiteSpace(this.settings.Username))
               {
                   store.Credentials = new NetworkCredential(this.settings.Username, this.settings.Password);
               }*/

            store.Initialize();

            return store;
        }


        private static long ProcessEventsFromCommit(IEnumerable<CommittedEvent> eventsFromSingleCommit, long sequence, Guid commitId)
        {
            throw new Exception("When processing commits origin should be correctly preserved. Please add such functionality if you need to process events.");

            var streamToSave = new UncommittedEventStream(commitId, origin: null);

            int currentFlushGroup = 0; 

            var answersValid = new List<Identity>();
            var answersInvalid = new List<Identity>();
            var questionsEnabled = new List<Identity>();
            var questionsDisabled = new List<Identity>();
            var groupsEnabled = new List<Identity>();
            var groupsDisabled = new List<Identity>();
            var answersRemoved = new List<Identity>();
            var rostersAdded = new List<AddedRosterInstance>();
            var rostersRemoved = new List<RosterInstance>();


            CommittedEvent tmpEvent = null;
            foreach (var committedEvent in eventsFromSingleCommit)
            {
                streamToSave.Append(new UncommittedEvent(committedEvent.EventIdentifier, committedEvent.EventSourceId, sequence + 1, 1,
                    committedEvent.EventTimeStamp, committedEvent.Payload, committedEvent.EventVersion));
                sequence++;

                answersValid = new List<Identity>();
                answersInvalid = new List<Identity>();
                questionsEnabled = new List<Identity>();
                questionsDisabled = new List<Identity>();
                groupsEnabled = new List<Identity>();
                groupsDisabled = new List<Identity>();
                answersRemoved = new List<Identity>();
                rostersAdded = new List<AddedRosterInstance>();
                rostersRemoved = new List<RosterInstance>();
                tmpEvent = null;
            }

            eventStoreTarget.Store(streamToSave);
            return sequence;
        }

        private static void AddrIgnore(List<Identity> list, Guid id, decimal[] vector)
        {
            if (list.Any(identity => identity.Id == id && identity.RosterVector.SequenceEqual(vector)))
                return;
            list.Add(new Identity(id, vector));
        }

        private static long AddOrIgnoreAndReturnNewSequence<T, TBase>(UncommittedEventStream stream, IEnumerable<CommittedEvent> eventsFromSingleCommit, CommittedEvent baseEvent, Func<T> creator, long sequence) where T : class
        {
            var newSequence = sequence + 1;

            var eventFromStream = new UncommittedEvent(baseEvent.EventIdentifier, baseEvent.EventSourceId, newSequence, 1,
                baseEvent.EventTimeStamp, creator(), baseEvent.EventVersion);
            stream.Append(eventFromStream);
            return newSequence;
        }
    }
}
