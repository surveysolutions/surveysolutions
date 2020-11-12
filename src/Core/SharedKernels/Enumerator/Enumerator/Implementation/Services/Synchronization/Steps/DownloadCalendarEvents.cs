using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps
{
    public class DownloadCalendarEvents : SynchronizationStep
    {
        protected readonly IEnumeratorEventStorage EventStore;
        private readonly ILiteEventBus eventBus;
        private readonly ICalendarEventStorage calendarEventStorage;
        private readonly ILogger log;
        
        public DownloadCalendarEvents(int sortOrder, ISynchronizationService synchronizationService, 
            ILogger logger, IEnumeratorEventStorage eventStore,
            ICalendarEventStorage calendarEventStorage, ILiteEventBus eventBus, ILogger log) 
            : base(sortOrder, synchronizationService, logger)
        {
            EventStore = eventStore;
            this.eventBus = eventBus;
            this.log = log;
            this.calendarEventStorage = calendarEventStorage;
        }

        public override async Task ExecuteAsync()
        {
            List<CalendarEventApiView> remoteCalendarEvents = 
                await this.synchronizationService.GetCalendarEventsAsync(this.Context.CancellationToken);

            var remoteCalendarEventsWithSequence = 
                remoteCalendarEvents.ToDictionary(x => x.CalendarEventId, 
                    x => x.Sequence);
            
            // all local created are known on server
            // 1. delete all not from list
            // 2. if server last event sequence is bigger than local
            //    drop local and recreate
            // 3. download all not existing locally or needed to be recreated
            
            var localCalendarEvents = calendarEventStorage.LoadAll();
            var localCalendarEventIds = localCalendarEvents.Select(ce => ce.Id).ToHashSet();

            var localCalendarEventsToRemove = localCalendarEventIds.Where(
                ce => !remoteCalendarEventsWithSequence.ContainsKey(ce)).ToList(); 
            
            //all calendar events should be pushed at this point
            var localCalendarEventsToRecreate = localCalendarEventIds.Where(
                ce => remoteCalendarEventsWithSequence.ContainsKey(ce) &&
                      EventStore.GetLastEventSequence(ce) != remoteCalendarEventsWithSequence[ce]).ToList();

            var remoteCalendarEventsToDownload = remoteCalendarEventsWithSequence.Keys.Where(
                ce => !localCalendarEventIds.Contains(ce));
            
            IProgress<TransferProgress> transferProgress = this.Context.Progress.AsTransferReport();
            
            foreach (var localCalendarEvent in localCalendarEventsToRemove)
            {
                this.RemoveCalendarEvent(localCalendarEvent);
            }
            
            foreach (var localCalendarEvent in localCalendarEventsToRecreate)
            {
                this.RemoveCalendarEvent(localCalendarEvent);
                await this.DownloadAndCreateCalendarEvent(localCalendarEvent, transferProgress);
            }
            
            foreach (var localCalendarEvent in remoteCalendarEventsToDownload)
            {
                await this.DownloadAndCreateCalendarEvent(localCalendarEvent, transferProgress);
            }
        }

        private void RemoveCalendarEvent(Guid id)
        {
            calendarEventStorage.Remove(id);
            EventStore.RemoveEventSourceById(id);
        }

        private async Task DownloadAndCreateCalendarEvent(Guid id, IProgress<TransferProgress> transferProgress)
        {
            try
            {
                var calendarEventStream = await this.synchronizationService.GetCalendarEventStreamAsync(
                    id,
                    0,
                    transferProgress, 
                    Context.CancellationToken);
                
                this.log.Debug($"Creating calendar event {id}");
                
                EventStore.StoreEvents(new CommittedEventStream(id, calendarEventStream));
                eventBus.PublishCommittedEvents(calendarEventStream);
            }
            catch (OperationCanceledException)
            {

            }
        }
    }
}
